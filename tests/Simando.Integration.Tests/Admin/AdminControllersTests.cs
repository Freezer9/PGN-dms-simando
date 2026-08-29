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
using Simando.Application.MasterData;
using Simando.Application.Organisation;
using Simando.Application.Security;
using Simando.Application.Workflow;
using Simando.Domain.MasterData;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Admin;

public class AdminControllersTests : IAsyncLifetime
{
    private const string AdminEmail = "admin@pgn.co.id";
    private const string AdminInitialPassword = "Correct-Horse-Battery-Staple-1";
    private const string AdminPassword = "New-Correct-Horse-Password-1";

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new(System.Text.Json.JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgis/postgis:18-3.6-alpine")
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

            await db.SaveChangesAsync();
        }

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Sign in as admin
        await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(AdminEmail, AdminInitialPassword));
        await _client.PostAsJsonAsync("/api/auth/change-password", new ChangePasswordRequest(AdminInitialPassword, AdminPassword));
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact(DisplayName = "GET /api/admin/users returns list of users")]
    public async Task GetUsers_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/admin/users");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var users = await response.Content.ReadFromJsonAsync<List<UserListItemDto>>(JsonOptions);
        users.ShouldNotBeNull();
        users.Count.ShouldBeGreaterThan(0);
        users.ShouldContain(u => u.Email == AdminEmail);
    }

    [Fact(DisplayName = "POST /api/admin/users creates a new user and returns temporary password")]
    public async Task CreateUser_ReturnsTemporaryPassword()
    {
        var request = new CreateUserRequest(
            FullName: "Test Sales User",
            Username: "test.sales",
            Email: "test.sales@pgn.co.id",
            Role: Role.SalesArea
        );

        var response = await _client.PostAsJsonAsync("/api/admin/users", request, JsonOptions);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CreateUserResponse>(JsonOptions);
        result.ShouldNotBeNull();
        result.Username.ShouldBe("test.sales");
        result.TemporaryPassword.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "GET /api/admin/organisation returns hierarchy of regions and areas")]
    public async Task GetOrganisation_ReturnsRegionsAndAreas()
    {
        var response = await _client.GetAsync("/api/admin/organisation");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var org = await response.Content.ReadFromJsonAsync<List<RegionWithAreasDto>>(JsonOptions);
        org.ShouldNotBeNull();
        org.Count.ShouldBeGreaterThan(0);
    }

    [Fact(DisplayName = "POST & PUT & DELETE /api/admin/master/industry-types performs complete CRUD")]
    public async Task IndustryTypes_Crud_Works()
    {
        // 1. Create
        var createReq = new CreateIndustryTypeRequest("Tekstil Baru", "Kain katun");
        var postRes = await _client.PostAsJsonAsync("/api/admin/master/industry-types", createReq, JsonOptions);
        postRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        var created = await postRes.Content.ReadFromJsonAsync<IndustryTypeDto>(JsonOptions);
        created.ShouldNotBeNull();
        created.Name.ShouldBe("Tekstil Baru");

        // 2. Update
        var updateReq = new UpdateIndustryTypeRequest("Tekstil Modern", "Kain sintetis");
        var putRes = await _client.PutAsJsonAsync($"/api/admin/master/industry-types/{created.Id}", updateReq, JsonOptions);
        putRes.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // 3. Delete
        var delRes = await _client.DeleteAsync($"/api/admin/master/industry-types/{created.Id}");
        delRes.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "GET /api/admin/break-glass/logs returns paged logs")]
    public async Task BreakGlass_Workflow_Works()
    {
        var getLogs = await _client.GetAsync("/api/admin/break-glass/logs?page=1&pageSize=10");
        getLogs.StatusCode.ShouldBe(HttpStatusCode.OK);

        var logs = await getLogs.Content.ReadFromJsonAsync<PagedResult<BreakGlassAccessDto>>(JsonOptions);
        logs.ShouldNotBeNull();
    }

    [Fact(DisplayName = "GET /api/admin/stuck-steps returns cross-region stuck tasks")]
    public async Task GetStuckSteps_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/admin/stuck-steps");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var items = await response.Content.ReadFromJsonAsync<List<StuckStepItemDto>>(JsonOptions);
        items.ShouldNotBeNull();
    }
}
