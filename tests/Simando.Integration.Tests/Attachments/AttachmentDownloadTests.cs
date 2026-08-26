using System.Net;
using System.Text.RegularExpressions;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Simando.Application.Storage;
using Simando.Domain.Attachments;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Attachments;

// AttachmentsController is the one download path every attachment goes
// through (AGENTS.md: "no pre-signed storage URLs ... always stream through
// an authorising endpoint that re-checks scope"), and P15 in
// docs/build/testing.md ("attachment download by id -> 403" for an
// out-of-scope record) was previously untested anywhere in the suite.
// Full HTTP pipeline, real sign-in cookie, real MinIO bytes — same shape as
// SignInFlowTests, plus a real Storage:S3 backend per AttachmentStoreContractTests.
public class AttachmentDownloadTests : IAsyncLifetime
{
    private const string Bucket = "simando-attachment-test";
    private const string Password = "Correct-Horse-Battery-Staple-1";

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgis/postgis:18-3.6-alpine")
        .WithDatabase("simando")
        .WithUsername("simando")
        .WithPassword("simando")
        .Build();

    private readonly MinioContainer _minioContainer = new MinioBuilder("minio/minio:RELEASE.2023-01-31T02-24-19Z").Build();

    private WebApplicationFactory<Program> _factory = null!;
    private Seed _seed = null!;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_dbContainer.StartAsync(), _minioContainer.StartAsync());

        using var setupClient = new AmazonS3Client(
            new BasicAWSCredentials(_minioContainer.GetAccessKey(), _minioContainer.GetSecretKey()),
            new AmazonS3Config { ServiceURL = _minioContainer.GetConnectionString(), ForcePathStyle = true });
        await setupClient.PutBucketAsync(Bucket);

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder
                .ConfigureAppConfiguration((_, config) =>
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Postgres"] = _dbContainer.GetConnectionString(),
                        ["Storage:Type"] = "S3",
                        ["Storage:S3:ServiceUrl"] = _minioContainer.GetConnectionString(),
                        ["Storage:S3:Bucket"] = Bucket,
                        ["Storage:S3:ForcePathStyle"] = "true",
                        ["Storage:S3:AccessKey"] = _minioContainer.GetAccessKey(),
                        ["Storage:S3:SecretKey"] = _minioContainer.GetSecretKey(),
                    }))
                // Background hosted services (Hangfire server, notification
                // sender, ...) aren't this test's concern and need config
                // this test doesn't set up — same exclusion as SignInFlowTests.
                .ConfigureServices(services => services.RemoveAll<IHostedService>()));

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
            await db.Database.MigrateAsync();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            // The app's own IAttachmentStore, not a bare AmazonS3Client — it
            // carries the RequestChecksumCalculation workaround MinIO needs
            // (see S3AttachmentStore's constructor comment); a raw client
            // trips "x-amz-content-sha256 header does not match what was computed".
            var store = scope.ServiceProvider.GetRequiredService<IAttachmentStore>();
            _seed = await SeedAsync(db, userManager, store);
        }
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await Task.WhenAll(_dbContainer.DisposeAsync().AsTask(), _minioContainer.DisposeAsync().AsTask());
    }

    [Fact(DisplayName = "In-scope Sales Area downloads their own Area's attachment: 200, byte-identical content")]
    public async Task InScopeAttachment_Downloads200WithMatchingContent()
    {
        var client = await SignedInClientAsync(_seed.SalesAreaEmail);

        var response = await client.GetAsync($"/attachments/{_seed.OwnAreaAttachmentId}/download");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/pdf");
        (await response.Content.ReadAsByteArrayAsync()).ShouldBe(Seed.OwnAreaContent);
    }

    [Fact(DisplayName = "P15: attachment download by id for a record outside the actor's Area is 403, not 404")]
    public async Task OutOfScopeAttachment_Returns403()
    {
        var client = await SignedInClientAsync(_seed.SalesAreaEmail);

        var response = await client.GetAsync($"/attachments/{_seed.OtherAreaAttachmentId}/download");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "System Admin gets 403 on a company-scoped attachment even though their scope is All — no case-data bypass")]
    public async Task SystemAdmin_HasNoCaseDataBypass()
    {
        var client = await SignedInClientAsync(_seed.SystemAdminEmail);

        var response = await client.GetAsync($"/attachments/{_seed.OwnAreaAttachmentId}/download");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Unknown attachment id is 404")]
    public async Task UnknownAttachmentId_Returns404()
    {
        var client = await SignedInClientAsync(_seed.SalesAreaEmail);

        var response = await client.GetAsync($"/attachments/{Guid.NewGuid()}/download");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "An admin-uploaded attachment with no CompanyId (e.g. a reference document) skips the company-scope check entirely")]
    public async Task NoCompanyAttachment_SkipsScopeCheck()
    {
        var client = await SignedInClientAsync(_seed.SalesAreaEmail);

        var response = await client.GetAsync($"/attachments/{_seed.UnscopedAttachmentId}/download");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsByteArrayAsync()).ShouldBe(Seed.UnscopedContent);
    }

    private async Task<HttpClient> SignedInClientAsync(string email)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var token = await GetAntiforgeryTokenAsync(client, "/sign-in");
        var form = new Dictionary<string, string>
        {
            ["email"] = email,
            ["password"] = Password,
            ["__RequestVerificationToken"] = token,
        };

        var signIn = await client.PostAsync("/account/sign-in", new FormUrlEncodedContent(form));
        signIn.StatusCode.ShouldBe(HttpStatusCode.Redirect, $"sign-in as {email} did not succeed");

        return client;
    }

    private static async Task<string> GetAntiforgeryTokenAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        var html = await response.Content.ReadAsStringAsync();

        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        match.Success.ShouldBeTrue($"Could not find the antiforgery token on {path} (status {response.StatusCode}):\n{html}");

        return match.Groups[1].Value;
    }

    private sealed record Seed(
        string SalesAreaEmail,
        string SystemAdminEmail,
        Guid OwnAreaAttachmentId,
        Guid OtherAreaAttachmentId,
        Guid UnscopedAttachmentId)
    {
        public static readonly byte[] OwnAreaContent = "own-area attachment bytes"u8.ToArray();
        public static readonly byte[] OtherAreaContent = "other-area attachment bytes"u8.ToArray();
        public static readonly byte[] UnscopedContent = "reference document bytes"u8.ToArray();
    }

    private static async Task<Seed> SeedAsync(SimandoDbContext db, UserManager<ApplicationUser> userManager, IAttachmentStore store)
    {
        var provinceId = Guid.NewGuid();
        var regencyId = Guid.NewGuid();
        var districtId = Guid.NewGuid();
        var villageId = Guid.NewGuid();
        var industryTypeId = Guid.NewGuid();
        var regionId = Guid.NewGuid();
        var ownAreaId = Guid.NewGuid();
        var otherAreaId = Guid.NewGuid();

        db.Provinces.Add(new Province { Id = provinceId, BpsCode = "11", Name = "Test Province" });
        db.Regencies.Add(new Regency { Id = regencyId, ProvinceId = provinceId, BpsCode = "1101", Type = RegencyType.Kabupaten, Name = "Test Regency" });
        db.Districts.Add(new District { Id = districtId, RegencyId = regencyId, BpsCode = "110101", Name = "Test District" });
        db.Villages.Add(new Village { Id = villageId, DistrictId = districtId, BpsCode = "1101012001", Type = VillageType.Desa, Name = "Test Village" });
        db.IndustryTypes.Add(new IndustryType { Id = industryTypeId, Name = $"Test Industry {Guid.NewGuid():N}" });
        db.Regions.Add(new Region { Id = regionId, Code = Guid.NewGuid().ToString("N")[..8], Name = "Test Region", Active = true });
        db.Areas.Add(new Area { Id = ownAreaId, RegionId = regionId, Code = Guid.NewGuid().ToString("N")[..8], Name = "Own Area", Active = true });
        db.Areas.Add(new Area { Id = otherAreaId, RegionId = regionId, Code = Guid.NewGuid().ToString("N")[..8], Name = "Other Area", Active = true });

        var creatorId = Guid.NewGuid();
        db.Users.Add(new ApplicationUser { Id = creatorId, UserName = "seed-creator", FullName = "Seed Creator" });

        var ownCompanyId = Guid.NewGuid();
        var otherCompanyId = Guid.NewGuid();
        db.Companies.Add(NewCompany(ownCompanyId, "OWN-001", villageId, industryTypeId, ownAreaId, creatorId));
        db.Companies.Add(NewCompany(otherCompanyId, "OTHER-001", villageId, industryTypeId, otherAreaId, creatorId));

        var ownAttachmentId = Guid.NewGuid();
        var otherAttachmentId = Guid.NewGuid();
        var unscopedAttachmentId = Guid.NewGuid();
        db.Attachments.Add(NewAttachment(ownAttachmentId, ownCompanyId, "own.pdf", creatorId));
        db.Attachments.Add(NewAttachment(otherAttachmentId, otherCompanyId, "other.pdf", creatorId));
        db.Attachments.Add(NewAttachment(unscopedAttachmentId, companyId: null, "reference.pdf", creatorId));

        var salesArea = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "attach-test-salesarea",
            Email = "attach-test-salesarea@simando.local",
            FullName = "Attach Test Sales Area",
            MustChangePassword = false,
            Active = true,
        };
        (await userManager.CreateAsync(salesArea, Password)).Succeeded.ShouldBeTrue();
        db.RoleAssignments.Add(NewAssignment(salesArea.Id, Role.SalesArea, areaId: ownAreaId, regionId: null));

        var systemAdmin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "attach-test-sysadmin",
            Email = "attach-test-sysadmin@simando.local",
            FullName = "Attach Test System Admin",
            MustChangePassword = false,
            Active = true,
        };
        (await userManager.CreateAsync(systemAdmin, Password)).Succeeded.ShouldBeTrue();
        db.RoleAssignments.Add(NewAssignment(systemAdmin.Id, Role.SystemAdmin, areaId: null, regionId: null));

        await db.SaveChangesAsync();

        await store.PutAsync(new BlobWriteRequest(StorageKeyOf(ownAttachmentId), "application/pdf"), new MemoryStream(Seed.OwnAreaContent), CancellationToken.None);
        await store.PutAsync(new BlobWriteRequest(StorageKeyOf(otherAttachmentId), "application/pdf"), new MemoryStream(Seed.OtherAreaContent), CancellationToken.None);
        await store.PutAsync(new BlobWriteRequest(StorageKeyOf(unscopedAttachmentId), "application/pdf"), new MemoryStream(Seed.UnscopedContent), CancellationToken.None);

        return new Seed(
            salesArea.Email!,
            systemAdmin.Email!,
            ownAttachmentId,
            otherAttachmentId,
            unscopedAttachmentId);
    }

    private static string StorageKeyOf(Guid attachmentId) => $"test/{attachmentId}/v1/file.pdf";

    private static Attachment NewAttachment(Guid id, Guid? companyId, string filename, Guid uploadedBy) => new()
    {
        Id = id,
        CompanyId = companyId,
        AttachableType = companyId is null ? "reference_document" : "company",
        AttachableId = companyId ?? id,
        Kind = AttachmentKind.Kk0,
        Filename = filename,
        MimeType = "application/pdf",
        SizeBytes = 0,
        Checksum = "",
        StorageProvider = StorageProvider.S3,
        StorageKey = StorageKeyOf(id),
        UploadedBy = uploadedBy,
        UploadedAt = DateTimeOffset.UtcNow,
        Version = 1,
    };

    private static Company NewCompany(Guid id, string nomor, Guid villageId, Guid industryTypeId, Guid areaId, Guid creatorId) => new()
    {
        Id = id,
        Nomor = nomor,
        NamaPerusahaan = $"PT Test {nomor}",
        VillageId = villageId,
        Alamat = "Jl. Test",
        IndustryTypeId = industryTypeId,
        AreaId = areaId,
        CurrentStage = 1,
        Status = RecordStatus.Draft,
        CreatedBy = creatorId,
        CreatedAt = DateTimeOffset.UtcNow,
    };

    private static RoleAssignment NewAssignment(Guid userId, Role role, Guid? areaId, Guid? regionId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Role = role,
        AreaId = areaId,
        RegionId = regionId,
        Active = true,
        AssignedBy = userId,
        AssignedAt = DateTimeOffset.UtcNow,
    };
}
