using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Simando.Api.Controllers;
using Simando.Application.Common;
using Simando.Application.Directory;
using Simando.Application.Security;
using Simando.Application.Workflow;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Domain.Survey;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Security;

public class SystemAdminIsolationTests : IAsyncLifetime
{
    private const string AdminEmail = "admin@pgn.co.id";
    private const string AdminInitialPassword = "Admin-Pass-12345!";
    private const string AdminNewPassword = "Admin-New-Pass-12345!";

    private const string SalesEmail = "sales.admincheck@pgn.co.id";
    private const string SalesInitialPassword = "Correct-Horse-Battery-Staple-1";
    private const string SalesNewPassword = "New-Correct-Horse-Password-1";

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new(System.Text.Json.JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("imresamu/postgis:18-3.6-alpine")
        .WithDatabase("simando")
        .WithUsername("simando")
        .WithPassword("simando")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private Guid _regionId;
    private Guid _areaId;
    private Guid _villageId;
    private Guid _industryTypeId;
    private Guid _companyId;
    private Guid _salesUserId;
    private string _salesTemporaryPassword = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder
                .ConfigureAppConfiguration((_, config) =>
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Postgres"] = _container.GetConnectionString(),
                        ["Storage:Type"] = "S3",
                        ["Storage:S3:ServiceUrl"] = "http://localhost:9000",
                        ["Storage:S3:Bucket"] = "simando",
                        ["Storage:S3:AccessKey"] = "test",
                        ["Storage:S3:SecretKey"] = "test",
                    }))
                .ConfigureServices(services => services.RemoveAll<IHostedService>()));

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
        await db.Database.MigrateAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
        await seeder.SeedAsync("admin", AdminInitialPassword, "System Admin", email: AdminEmail);

        _regionId = Guid.NewGuid();
        _areaId = Guid.NewGuid();
        db.Regions.Add(new Region { Id = _regionId, Code = "SOR1", Name = "Region 1", Active = true });
        db.Areas.Add(new Area { Id = _areaId, RegionId = _regionId, Code = "JKT", Name = "Area Jakarta", Active = true });

        var province = new Province { Id = Guid.NewGuid(), BpsCode = "31", Name = "DKI Jakarta" };
        var regency = new Regency { Id = Guid.NewGuid(), ProvinceId = province.Id, BpsCode = "3171", Type = RegencyType.Kota, Name = "Jakarta Pusat" };
        var district = new District { Id = Guid.NewGuid(), RegencyId = regency.Id, BpsCode = "317101", Name = "Gambir" };
        var village = new Village { Id = Guid.NewGuid(), DistrictId = district.Id, BpsCode = "3171011001", Type = VillageType.Kelurahan, Name = "Gambir" };
        _villageId = village.Id;

        db.Provinces.Add(province);
        db.Regencies.Add(regency);
        db.Districts.Add(district);
        db.Villages.Add(village);

        var industryType = new IndustryType { Id = Guid.NewGuid(), Name = "Tekstil" };
        _industryTypeId = industryType.Id;
        db.IndustryTypes.Add(industryType);

        await db.SaveChangesAsync();

        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var adminActor = new EffectivePermissions(AccessScope.All, null, null, Enum.GetValues<Capability>().ToHashSet());
        var createSalesResult = await userService.CreateUserAsync(
            "Sales Jakarta", "sales.jkt", SalesEmail,
            Role.SalesArea, _areaId, _regionId,
            Guid.Empty, adminActor);

        _salesUserId = createSalesResult.UserId;
        _salesTemporaryPassword = createSalesResult.TemporaryPassword!;

        // Seed a commercial company owned by Sales Area
        _companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = _companyId,
            NomorSeq = 901,
            Nomor = "0901-31-3171",
            NamaPerusahaan = "PT Tekstil Nusantara",
            VillageId = _villageId,
            Alamat = "Jl. Merdeka Barat No. 1",
            IndustryTypeId = _industryTypeId,
            AreaId = _areaId,
            CurrentStage = 1,
            Status = RecordStatus.Draft,
            CreatedBy = _salesUserId,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Companies.Add(company);

        var survey = new Survey
        {
            CompanyId = _companyId,
            TanggalSurvey = DateOnly.FromDateTime(DateTime.UtcNow),
            JumlahKaryawan = 50,
            JumlahShift = 2,
            JamKerjaPerHari = 16,
            HariPerMinggu = 6
        };
        db.Surveys.Add(survey);

        db.CompanyContacts.Add(new CompanyContact
        {
            Id = Guid.NewGuid(),
            CompanyId = _companyId,
            Nama = "Budi Hartono",
            Jabatan = "Direktur Operasional",
            IsPrimary = true,
            SortOrder = 1,
            Email = "budi@nusantara.co.id",
            NoHp = "08123456789"
        });

        await db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    private async Task<HttpClient> CreateAuthenticatedAdminClientAsync()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var loginRes = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(AdminEmail, AdminInitialPassword));
        loginRes.EnsureSuccessStatusCode();

        var changeRes = await client.PostAsJsonAsync("/api/auth/change-password", new ChangePasswordRequest(AdminInitialPassword, AdminNewPassword));
        if (changeRes.IsSuccessStatusCode)
        {
            // Re-login with new password if password change succeeded
            var relogin = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(AdminEmail, AdminNewPassword));
            relogin.EnsureSuccessStatusCode();
        }

        return client;
    }

    [Fact(DisplayName = "System Admin without Break-Glass is 403 Forbidden from all commercial case data")]
    public async Task SystemAdmin_WithoutBreakGlass_IsForbiddenFromCommercialData()
    {
        using var client = await CreateAuthenticatedAdminClientAsync();

        // 1. Directory list
        var listRes = await client.GetAsync("/api/companies");
        listRes.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // 2. Map pins
        var mapRes = await client.GetAsync("/api/companies/map-pins");
        mapRes.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // 3. Company Hub Record detail
        var recordRes = await client.GetAsync($"/api/companies/{_companyId}");
        recordRes.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // 4. Survey stage
        var surveyRes = await client.GetAsync($"/api/companies/{_companyId}/survey");
        surveyRes.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // 5. Plotting stage
        var plottingRes = await client.GetAsync($"/api/companies/{_companyId}/plotting");
        plottingRes.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // 6. Registration stage
        var regRes = await client.GetAsync($"/api/companies/{_companyId}/registration");
        regRes.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // 7. NOL Request stage
        var nolRes = await client.GetAsync($"/api/companies/{_companyId}/nol-request");
        nolRes.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // 8. Contacts
        var contactsRes = await client.GetAsync($"/api/companies/{_companyId}/contacts");
        contactsRes.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // 9. Attachments
        var attachRes = await client.GetAsync($"/api/companies/{_companyId}/attachments");
        attachRes.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // 10. Timeline
        var timelineRes = await client.GetAsync($"/api/companies/{_companyId}/timeline");
        timelineRes.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "System Admin with Break-Glass receives audited read-only access")]
    public async Task SystemAdmin_BreakGlass_GrantsAuditedReadOnlyAccess()
    {
        using var client = await CreateAuthenticatedAdminClientAsync();

        // Attempt without reason is rejected
        var invalidReq = await client.PostAsJsonAsync("/api/admin/break-glass/request", new BreakGlassRequest(_companyId, ""));
        invalidReq.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Request break-glass with reason
        var validReq = await client.PostAsJsonAsync("/api/admin/break-glass/request", new BreakGlassRequest(_companyId, "Investigasi kendala teknis database"));
        validReq.StatusCode.ShouldBe(HttpStatusCode.OK);
        var accessDto = await validReq.Content.ReadFromJsonAsync<BreakGlassAccessDto>(JsonOptions);
        accessDto.ShouldNotBeNull();
        accessDto.IsActive.ShouldBeTrue();

        // Now System Admin can read company record in read-only mode
        var recordRes = await client.GetAsync($"/api/companies/{_companyId}");
        recordRes.StatusCode.ShouldBe(HttpStatusCode.OK);
        var record = await recordRes.Content.ReadFromJsonAsync<CompanyRecordDto>(JsonOptions);
        record.ShouldNotBeNull();
        record.NamaPerusahaan.ShouldBe("PT Tekstil Nusantara");
        record.CanSubmit.ShouldBeFalse();
        record.CanAct.ShouldBeFalse();

        // Can read survey
        var surveyRes = await client.GetAsync($"/api/companies/{_companyId}/survey");
        surveyRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Can read contacts
        var contactsRes = await client.GetAsync($"/api/companies/{_companyId}/contacts");
        contactsRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Can read timeline
        var timelineRes = await client.GetAsync($"/api/companies/{_companyId}/timeline");
        timelineRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Audit log reflects the emergency access
        var logsRes = await client.GetAsync("/api/admin/break-glass/logs");
        logsRes.StatusCode.ShouldBe(HttpStatusCode.OK);
        var logs = await logsRes.Content.ReadFromJsonAsync<PagedResult<BreakGlassAccessDto>>(JsonOptions);
        logs.ShouldNotBeNull();
        logs.Items.Any(l => l.CompanyId == _companyId && l.Reason.Contains("Investigasi kendala teknis")).ShouldBeTrue();
    }

    [Fact(DisplayName = "System Admin can soft-delete a Draft company across regions")]
    public async Task SystemAdmin_CanSoftDeleteDraftCompany()
    {
        using var client = await CreateAuthenticatedAdminClientAsync();

        var deleteRes = await client.DeleteAsync($"/api/companies/{_companyId}");
        deleteRes.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
        var company = await db.Companies.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == _companyId);
        company.ShouldNotBeNull();
        company.DeletedAt.ShouldNotBeNull();
    }

    [Fact(DisplayName = "System Admin cannot soft-delete a company that has entered workflow")]
    public async Task SystemAdmin_CannotSoftDeleteSubmittedCompany()
    {
        var submittedCompanyId = Guid.NewGuid();
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
            db.Companies.Add(new Company
            {
                Id = submittedCompanyId,
                NomorSeq = 902,
                Nomor = "0902-31-3171",
                NamaPerusahaan = "PT Tekstil Submitted",
                VillageId = _villageId,
                Alamat = "Jl. Merdeka Barat No. 2",
                IndustryTypeId = _industryTypeId,
                AreaId = _areaId,
                CurrentStage = 6,
                Status = RecordStatus.AreaHead,
                CreatedBy = _salesUserId,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        using var client = await CreateAuthenticatedAdminClientAsync();
        var deleteRes = await client.DeleteAsync($"/api/companies/{submittedCompanyId}");
        deleteRes.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await deleteRes.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        problem.ShouldNotBeNull();
        problem.Detail!.ShouldContain("Berkas yang pernah diajukan tidak dapat dihapus");
    }

    [Fact(DisplayName = "Sales Area role cannot request Break-Glass emergency access")]
    public async Task SalesArea_CannotRequestBreakGlass()
    {
        var salesClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var loginRes = await salesClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(SalesEmail, _salesTemporaryPassword));
        loginRes.EnsureSuccessStatusCode();

        var changeRes = await salesClient.PostAsJsonAsync("/api/auth/change-password", new ChangePasswordRequest(_salesTemporaryPassword, SalesNewPassword));
        if (changeRes.IsSuccessStatusCode)
        {
            var relogin = await salesClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(SalesEmail, SalesNewPassword));
            relogin.EnsureSuccessStatusCode();
        }

        var reqRes = await salesClient.PostAsJsonAsync("/api/admin/break-glass/request", new BreakGlassRequest(_companyId, "Alasan tidak berizin"));
        reqRes.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "System Admin can view cross-region stuck steps without case-data visibility")]
    public async Task SystemAdmin_CanViewCrossRegionStuckSteps()
    {
        using var client = await CreateAuthenticatedAdminClientAsync();

        var stuckRes = await client.GetAsync("/api/admin/stuck-steps");
        stuckRes.StatusCode.ShouldBe(HttpStatusCode.OK);
        var stuckList = await stuckRes.Content.ReadFromJsonAsync<IReadOnlyList<StuckStepItemDto>>(JsonOptions);
        stuckList.ShouldNotBeNull();
    }
}
