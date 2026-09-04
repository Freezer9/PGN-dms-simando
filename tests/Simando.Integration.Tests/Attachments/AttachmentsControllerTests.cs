using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
using Simando.Application.Attachments;
using Simando.Application.Storage;
using Simando.Domain.Attachments;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Organisation;
using Simando.Domain.Registration;
using Simando.Domain.Security;
using Simando.Domain.Survey;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Attachments;

public class AttachmentsControllerTests : IAsyncLifetime
{
    private const string Bucket = "simando-attachment-crud-test";
    private const string Password = "Correct-Horse-Battery-Staple-1";

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("imresamu/postgis:18-3.6-alpine")
        .WithDatabase("simando")
        .WithUsername("simando")
        .WithPassword("simando")
        .Build();

    private readonly MinioContainer _minioContainer = new MinioBuilder("minio/minio:RELEASE.2023-01-31T02-24-19Z").Build();

    private WebApplicationFactory<Program> _factory = null!;
    private Guid _companyId;
    private string _salesAreaEmail = null!;

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
                .ConfigureServices(services => services.RemoveAll<IHostedService>()));

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
            await db.Database.MigrateAsync();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await SeedDataAsync(db, userManager);
        }
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await Task.WhenAll(_dbContainer.DisposeAsync().AsTask(), _minioContainer.DisposeAsync().AsTask());
    }

    [Fact(DisplayName = "Upload attachment via multipart form and list company attachments")]
    public async Task UploadAndListAttachments_Succeeds()
    {
        var client = await SignedInClientAsync(_salesAreaEmail);

        // Upload attachment
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent("test pdf content"u8.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(fileContent, "file", "document_test.pdf");
        form.Add(new StringContent("Kk0"), "kind");
        form.Add(new StringContent("Digital"), "signatureMethod");

        var uploadResponse = await client.PostAsync($"/api/companies/{_companyId}/attachments", form);
        uploadResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        var uploaded = await uploadResponse.Content.ReadFromJsonAsync<AttachmentDetail>(jsonOptions);
        uploaded.ShouldNotBeNull();
        uploaded.Filename.ShouldBe("document_test.pdf");
        uploaded.Kind.ShouldBe(AttachmentKind.Kk0);
        uploaded.Version.ShouldBe(1);

        // List attachments
        var listResponse = await client.GetAsync($"/api/companies/{_companyId}/attachments");
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var list = await listResponse.Content.ReadFromJsonAsync<List<AttachmentDetail>>(jsonOptions);
        list.ShouldNotBeNull();
        list.Count.ShouldBeGreaterThanOrEqualTo(1);
        list.ShouldContain(a => a.Id == uploaded.Id && a.Filename == "document_test.pdf");

        // Download document generation KK0
        var docResponse = await client.GetAsync($"/api/documents/company/{_companyId}/kk0");
        docResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        docResponse.Content.Headers.ContentType?.MediaType.ShouldBe("application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        // Delete attachment
        var deleteResponse = await client.DeleteAsync($"/api/attachments/{uploaded.Id}");
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    private async Task<HttpClient> SignedInClientAsync(string email)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.PostAsJsonAsync("/api/auth/login", new Simando.Api.Controllers.LoginRequest(email, Password));
        response.StatusCode.ShouldBe(HttpStatusCode.OK, $"sign-in as {email} did not succeed");

        return client;
    }

    private async Task SeedDataAsync(SimandoDbContext db, UserManager<ApplicationUser> userManager)
    {
        var provinceId = Guid.NewGuid();
        var regencyId = Guid.NewGuid();
        var districtId = Guid.NewGuid();
        var villageId = Guid.NewGuid();
        var industryTypeId = Guid.NewGuid();
        var regionId = Guid.NewGuid();
        var areaId = Guid.NewGuid();

        db.Provinces.Add(new Province { Id = provinceId, BpsCode = "12", Name = "Sumatera Utara" });
        db.Regencies.Add(new Regency { Id = regencyId, ProvinceId = provinceId, BpsCode = "1201", Type = RegencyType.Kota, Name = "Medan" });
        db.Districts.Add(new District { Id = districtId, RegencyId = regencyId, BpsCode = "120101", Name = "Medan Kota" });
        db.Villages.Add(new Village { Id = villageId, DistrictId = districtId, BpsCode = "1201012001", Type = VillageType.Kelurahan, Name = "Pasar Merah" });
        db.IndustryTypes.Add(new IndustryType { Id = industryTypeId, Name = "Industri Kimia" });
        db.Regions.Add(new Region { Id = regionId, Code = "REG1", Name = "Region 1 - Sumbagut", Active = true });
        db.Areas.Add(new Area { Id = areaId, RegionId = regionId, Code = "MEDAN", Name = "Area Medan", Active = true });

        var salesUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "sales.medan",
            Email = "sales.medan@simando.local",
            FullName = "Budi Sales Medan",
            MustChangePassword = false,
            Active = true,
        };
        (await userManager.CreateAsync(salesUser, Password)).Succeeded.ShouldBeTrue();
        db.RoleAssignments.Add(new RoleAssignment
        {
            Id = Guid.NewGuid(),
            UserId = salesUser.Id,
            Role = Role.SalesArea,
            AreaId = areaId,
            RegionId = null,
            Active = true,
            AssignedBy = salesUser.Id,
            AssignedAt = DateTimeOffset.UtcNow,
        });

        _companyId = Guid.NewGuid();
        db.Companies.Add(new Company
        {
            Id = _companyId,
            Nomor = "1-12-1201",
            NamaPerusahaan = "PT Kimia Sejahtera",
            VillageId = villageId,
            Alamat = "Jl. Industri No. 45",
            IndustryTypeId = industryTypeId,
            AreaId = areaId,
            CurrentStage = 1,
            Status = RecordStatus.Draft,
            CreatedBy = salesUser.Id,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        db.Surveys.Add(new Survey
        {
            CompanyId = _companyId,
            TanggalSurvey = DateOnly.FromDateTime(DateTime.UtcNow),
            KeteranganLain = "Survey awal",
        });

        await db.SaveChangesAsync();

        _salesAreaEmail = salesUser.Email!;
    }
}
