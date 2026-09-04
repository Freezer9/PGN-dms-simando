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
using Simando.Application.Tasks;
using Simando.Domain.Audit;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Tasks;

public class TasksControllerTests : IAsyncLifetime
{
    private const string AdminEmail = "admin@pgn.co.id";
    private const string AdminInitialPassword = "Correct-Horse-Battery-Staple-1";
    private const string AdminPassword = "New-Correct-Horse-Password-1";

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
            await seeder.SeedAsync("admin", AdminInitialPassword, "System Admin", email: AdminEmail);

            var user = await db.Users.FirstAsync(u => u.Email == AdminEmail);
            await SeedWorkflowDataAsync(db, user.Id);
        }

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Authenticate client
        await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(AdminEmail, AdminInitialPassword));
        await _client.PostAsJsonAsync("/api/auth/change-password", new ChangePasswordRequest(AdminInitialPassword, AdminPassword));
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact(DisplayName = "GET /api/tasks/inbox returns 200 OK with task list for logged in actor")]
    public async Task GetInbox_ReturnsTasks()
    {
        var response = await _client.GetAsync("/api/tasks/inbox");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<TaskListItem>>(JsonOptions);
        items.ShouldNotBeNull();
    }

    [Fact(DisplayName = "GET /api/tasks/summary returns 200 OK with counts")]
    public async Task GetSummary_ReturnsCounts()
    {
        var response = await _client.GetAsync("/api/tasks/summary");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var summary = await response.Content.ReadFromJsonAsync<TasksSummaryDto>(JsonOptions);
        summary.ShouldNotBeNull();
    }

    [Fact(DisplayName = "GET /api/tasks/history returns 200 OK with paged history")]
    public async Task GetHistory_ReturnsPagedHistory()
    {
        var response = await _client.GetAsync("/api/tasks/history?page=1&pageSize=10");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var history = await response.Content.ReadFromJsonAsync<PagedResult<TaskHistoryItem>>(JsonOptions);
        history.ShouldNotBeNull();
    }

    private static async Task SeedWorkflowDataAsync(SimandoDbContext db, Guid userId)
    {
        var provinceId = Guid.NewGuid();
        var regencyId = Guid.NewGuid();
        var districtId = Guid.NewGuid();
        var villageId = Guid.NewGuid();
        var industryTypeId = Guid.NewGuid();
        var regionId = Guid.NewGuid();
        var areaId = Guid.NewGuid();

        db.Provinces.Add(new Province { Id = provinceId, BpsCode = "35", Name = "Jawa Timur" });
        db.Regencies.Add(new Regency { Id = regencyId, ProvinceId = provinceId, BpsCode = "3578", Type = RegencyType.Kota, Name = "Surabaya" });
        db.Districts.Add(new District { Id = districtId, RegencyId = regencyId, BpsCode = "357801", Name = "Tegalsari" });
        db.Villages.Add(new Village { Id = villageId, DistrictId = districtId, BpsCode = "3578011001", Type = VillageType.Kelurahan, Name = "Keputran" });
        db.IndustryTypes.Add(new IndustryType { Id = industryTypeId, Name = "Manufacturing" });
        db.Regions.Add(new Region { Id = regionId, Code = "SOR3", Name = "Region 3 - Jatim Bali Nusa", Active = true });
        var area = new Area { Id = areaId, RegionId = regionId, Code = "SBY", Name = "Area Surabaya", Active = true };
        db.Areas.Add(area);

        db.RoleAssignments.Add(new RoleAssignment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Role = Role.AreaHead,
            AreaId = areaId,
            Active = true,
            AssignedBy = userId,
            AssignedAt = DateTimeOffset.UtcNow,
        });

        var company = new Company
        {
            Id = Guid.NewGuid(),
            Nomor = "1-35-3578",
            NamaPerusahaan = "PT Surya Gas",
            VillageId = villageId,
            Alamat = "Jl. Basuki Rahmat No. 1",
            IndustryTypeId = industryTypeId,
            AreaId = areaId,
            CurrentStage = 6,
            Status = RecordStatus.AreaHead,
            CreatedBy = userId,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.Companies.Add(company);

        var instance = new WorkflowInstance
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            StartedAt = DateTimeOffset.UtcNow,
        };
        db.WorkflowInstances.Add(instance);

        var step = new WorkflowStep
        {
            Id = Guid.NewGuid(),
            WorkflowInstanceId = instance.Id,
            Kind = WorkflowStepKind.AreaHead,
            StepOrder = 1,
            AssignedUserId = userId,
        };
        db.WorkflowSteps.Add(step);

        db.StatusEvents.Add(new StatusEvent
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            FromStage = 6,
            ToStage = 6,
            FromStatus = RecordStatus.Draft,
            ToStatus = RecordStatus.AreaHead,
            ActorId = userId,
            Action = StatusEventAction.Submit,
            OccurredAt = DateTimeOffset.UtcNow,
            WorkflowStepId = step.Id,
        });

        await db.SaveChangesAsync();
    }
}
