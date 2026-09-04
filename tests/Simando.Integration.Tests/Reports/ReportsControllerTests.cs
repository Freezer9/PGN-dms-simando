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
using Simando.Application.Reports;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Reports;

public class ReportsControllerTests : IAsyncLifetime
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

            var regionId = Guid.NewGuid();
            var areaId = Guid.NewGuid();
            db.Regions.Add(new Region { Id = regionId, Code = "SOR3", Name = "Region 3 - Jatim Bali Nusa", Active = true });
            db.Areas.Add(new Area { Id = areaId, RegionId = regionId, Code = "SBY", Name = "Area Surabaya", Active = true });

            var adminUser = await db.Users.FirstAsync(u => u.Email == AdminEmail);
            db.RoleAssignments.Add(new RoleAssignment
            {
                Id = Guid.NewGuid(),
                UserId = adminUser.Id,
                Role = Role.RegionalAdmin,
                RegionId = regionId,
                Active = true,
                AssignedAt = DateTimeOffset.UtcNow,
                AssignedBy = adminUser.Id
            });

            await db.SaveChangesAsync();
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

    [Fact(DisplayName = "GET /api/reports/funnel returns 200 OK with funnel data")]
    public async Task GetFunnel_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/reports/funnel");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<FunnelReportDto>(JsonOptions);
        report.ShouldNotBeNull();
        report.Stages.Count.ShouldBe(8);
    }

    [Fact(DisplayName = "GET /api/reports/gas-demand returns 200 OK with gas demand data")]
    public async Task GetGasDemand_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/reports/gas-demand");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<GasDemandReportDto>(JsonOptions);
        report.ShouldNotBeNull();
    }

    [Fact(DisplayName = "GET /api/reports/survey-productivity returns 200 OK")]
    public async Task GetSurveyProductivity_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/reports/survey-productivity");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<SurveyProductivityReportDto>(JsonOptions);
        report.ShouldNotBeNull();
    }

    [Fact(DisplayName = "GET /api/reports/nol-outcomes returns 200 OK")]
    public async Task GetNolOutcomes_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/reports/nol-outcomes");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<NolOutcomesReportDto>(JsonOptions);
        report.ShouldNotBeNull();
    }

    [Fact(DisplayName = "GET /api/reports/ageing returns 200 OK")]
    public async Task GetAgeing_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/reports/ageing");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<List<AgeingRow>>(JsonOptions);
        report.ShouldNotBeNull();
    }

    [Fact(DisplayName = "GET /api/reports/export/funnel returns 200 OK xlsx stream")]
    public async Task ExportFunnel_ReturnsXlsx()
    {
        var response = await _client.GetAsync("/api/reports/export/funnel");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [Fact(DisplayName = "GET /api/reports/export/ageing returns 200 OK xlsx stream")]
    public async Task ExportAgeing_ReturnsXlsx()
    {
        var response = await _client.GetAsync("/api/reports/export/ageing");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}
