using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Simando.Api.Controllers;
using Simando.Application.Common;
using Simando.Application.Security;
using Simando.Application.Workflow;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Workflow;

public class WorkflowControllerTests : IAsyncLifetime
{
    private const string ApproverEmail = "approver.wf@pgn.co.id";
    private const string ApproverInitialPassword = "Correct-Horse-Battery-Staple-1";
    private const string ApproverPassword = "New-Correct-Horse-Password-1";
    private const string SalesEmail = "sales.wf@pgn.co.id";

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
    private HttpClient _client = null!;
    private Guid _approverUserId;
    private string _approverTemporaryPassword = null!;
    private Guid _salesUserId;
    private Guid _companyId;
    private Guid _stepId;

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

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
            await db.Database.MigrateAsync();

            var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
            await seeder.SeedAsync("admin.wf", "Admin-Pass-12345!", "Admin WF", email: "admin.wf@pgn.co.id");

            var region = new Region { Id = Guid.NewGuid(), Code = "SOR2", Name = "Region 2", Active = true };
            var area = new Area { Id = Guid.NewGuid(), RegionId = region.Id, Code = "JKT", Name = "Area Jakarta", Active = true };
            db.Regions.Add(region);
            db.Areas.Add(area);

            var province = new Province { Id = Guid.NewGuid(), BpsCode = "31", Name = "DKI Jakarta" };
            var regency = new Regency { Id = Guid.NewGuid(), ProvinceId = province.Id, BpsCode = "3171", Type = RegencyType.Kota, Name = "Jakarta Pusat" };
            var district = new District { Id = Guid.NewGuid(), RegencyId = regency.Id, BpsCode = "317101", Name = "Gambir" };
            var village = new Village { Id = Guid.NewGuid(), DistrictId = district.Id, BpsCode = "3171011001", Type = VillageType.Kelurahan, Name = "Gambir" };
            db.Provinces.Add(province);
            db.Regencies.Add(regency);
            db.Districts.Add(district);
            db.Villages.Add(village);

            var industryType = new IndustryType { Id = Guid.NewGuid(), Name = "Tekstil" };
            db.IndustryTypes.Add(industryType);

            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var adminActor = new EffectivePermissions(AccessScope.All, null, null, Enum.GetValues<Capability>().ToHashSet());

            // 1. Create Sales user (creator)
            var salesResult = await userService.CreateUserAsync(
                "Sales Jakarta", "sales.jkt", SalesEmail,
                Role.SalesArea, area.Id, region.Id,
                Guid.Empty, adminActor);
            _salesUserId = salesResult.UserId;

            // 2. Create AreaHead user (approver)
            var createResult = await userService.CreateUserAsync(
                "Kepala Area Jakarta", "head.jkt", ApproverEmail,
                Role.AreaHead, area.Id, region.Id,
                Guid.Empty, adminActor);
            _approverUserId = createResult.UserId;
            _approverTemporaryPassword = createResult.TemporaryPassword!;

            await db.SaveChangesAsync();

            // 3. Create company prospect
            var company = new Company
            {
                Id = Guid.NewGuid(),
                NomorSeq = 1,
                Nomor = "0000001-31-3171",
                NamaPerusahaan = "PT Tekstil Megah",
                Alamat = "Jl. Gambir Raya No. 1",
                VillageId = village.Id,
                Location = new NetTopologySuite.Geometries.Point(106.8272, -6.1754) { SRID = 4326 },
                IndustryTypeId = industryType.Id,
                AreaId = area.Id,
                CurrentStage = 6,
                Status = RecordStatus.Draft,
                CreatedBy = _salesUserId,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            _companyId = company.Id;
            db.Companies.Add(company);

            db.Attachments.AddRange(
                new Simando.Domain.Attachments.Attachment
                {
                    Id = Guid.NewGuid(),
                    CompanyId = company.Id,
                    AttachableType = "Company",
                    AttachableId = company.Id,
                    Kind = Simando.Domain.Attachments.AttachmentKind.A1,
                    Filename = "a1.pdf",
                    MimeType = "application/pdf",
                    SizeBytes = 1024,
                    Checksum = "sha256-dummy-1",
                    StorageKey = "attachments/a1.pdf",
                    StorageProvider = Simando.Domain.Attachments.StorageProvider.S3,
                    UploadedBy = _salesUserId,
                    UploadedAt = DateTimeOffset.UtcNow,
                    Version = 1
                },
                new Simando.Domain.Attachments.Attachment
                {
                    Id = Guid.NewGuid(),
                    CompanyId = company.Id,
                    AttachableType = "Company",
                    AttachableId = company.Id,
                    Kind = Simando.Domain.Attachments.AttachmentKind.Kk0,
                    Filename = "kk0.pdf",
                    MimeType = "application/pdf",
                    SizeBytes = 1024,
                    Checksum = "sha256-dummy-2",
                    StorageKey = "attachments/kk0.pdf",
                    StorageProvider = Simando.Domain.Attachments.StorageProvider.S3,
                    UploadedBy = _salesUserId,
                    UploadedAt = DateTimeOffset.UtcNow,
                    Version = 1
                },
                new Simando.Domain.Attachments.Attachment
                {
                    Id = Guid.NewGuid(),
                    CompanyId = company.Id,
                    AttachableType = "Company",
                    AttachableId = company.Id,
                    Kind = Simando.Domain.Attachments.AttachmentKind.CapexPreGr3,
                    Filename = "capex.pdf",
                    MimeType = "application/pdf",
                    SizeBytes = 1024,
                    Checksum = "sha256-dummy-3",
                    StorageKey = "attachments/capex.pdf",
                    StorageProvider = Simando.Domain.Attachments.StorageProvider.S3,
                    UploadedBy = _salesUserId,
                    UploadedAt = DateTimeOffset.UtcNow,
                    Version = 1
                });

            await db.SaveChangesAsync();

            // 4. Start workflow as Sales User
            var wfService = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
            var salesActor = new EffectivePermissions(AccessScope.Area, area.Id, region.Id, new HashSet<Capability> { Capability.SubmitForApproval });
            var startResult = await wfService.StartAsync(_companyId, _salesUserId, salesActor, new HashSet<Role> { Role.SalesArea });
            startResult.Succeeded.ShouldBeTrue();

            var activeStep = await db.WorkflowSteps.IgnoreQueryFilters()
                .Where(s => s.WorkflowInstanceId == startResult.WorkflowInstanceId && s.Kind == WorkflowStepKind.AreaHead)
                .SingleAsync();
            _stepId = activeStep.Id;
        }

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Sign in as approver
        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(ApproverEmail, _approverTemporaryPassword));
        loginRes.EnsureSuccessStatusCode();
        var changeRes = await _client.PostAsJsonAsync("/api/auth/change-password", new ChangePasswordRequest(_approverTemporaryPassword, ApproverPassword));
        changeRes.EnsureSuccessStatusCode();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact(DisplayName = "POST /api/workflow/steps/{stepId}/act approves workflow step")]
    public async Task Act_Setuju_Succeeds()
    {
        var actRequest = new ActOnStepRequest(WorkflowAction.Setuju, "Survey disetujui, lokasi dan data valid.");

        var response = await _client.PostAsJsonAsync($"/api/workflow/steps/{_stepId}/act", actRequest);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();

        var step = await db.WorkflowSteps.IgnoreQueryFilters().SingleAsync(s => s.Id == _stepId);
        step.ActedAt.ShouldNotBeNull();
        step.Action.ShouldBe(WorkflowAction.Setuju);

        var company = await db.Companies.IgnoreQueryFilters().SingleAsync(c => c.Id == _companyId);
        company.Status.ShouldBe(RecordStatus.RegionalAdmin);
    }
}
