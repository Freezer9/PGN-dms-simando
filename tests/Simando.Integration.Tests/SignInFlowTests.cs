using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Simando.Api.Controllers;
using Simando.Domain.Security;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests;

// End-to-end Web API auth pipeline tests for Simando.Api.
public class SignInFlowTests : IAsyncLifetime
{
    private const string AdminUsername = "admin";
    private const string AdminEmail = "admin@pgn.co.id";
    private const string AdminPassword = "Correct-Horse-Battery-Staple-1";

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
            var seedResult = await seeder.SeedAsync(AdminUsername, AdminPassword, "System Admin", email: AdminEmail);
            seedResult.Outcome.ShouldBe(AdminSeedOutcome.Created);
        }

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact(DisplayName = "Unauthenticated request to protected endpoint returns 401 Unauthorized")]
    public async Task Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/api/auth/me");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Valid sign-in returns 200 OK with CurrentUserDto and sets auth cookie")]
    public async Task ValidSignIn_ReturnsUserDto_And_SetsCookie()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(AdminEmail, AdminPassword));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var userDto = await response.Content.ReadFromJsonAsync<CurrentUserDto>();
        userDto.ShouldNotBeNull();
        userDto.Email.ShouldBe(AdminEmail);
        userDto.MustChangePassword.ShouldBeTrue();
        userDto.Scope.ShouldBe(AccessScope.All);
        userDto.Roles.ShouldContain(Role.SystemAdmin.ToString());

        response.Headers.ShouldContain(h => h.Key == "Set-Cookie");
    }

    [Fact(DisplayName = "Wrong password and unknown email return 401 with identical ProblemDetails")]
    public async Task WrongPassword_And_UnknownEmail_RejectedIdentically()
    {
        var wrongPasswordResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(AdminEmail, "wrong-password"));
        var unknownUserResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("nonexistent@pgn.co.id", "wrong-password"));

        wrongPasswordResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        unknownUserResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var wrongDetails = await wrongPasswordResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        var unknownDetails = await unknownUserResponse.Content.ReadFromJsonAsync<ProblemDetails>();

        wrongDetails.ShouldNotBeNull();
        unknownDetails.ShouldNotBeNull();
        wrongDetails.Title.ShouldBe(unknownDetails.Title);
        wrongDetails.Detail.ShouldBe(unknownDetails.Detail);
    }

    [Fact(DisplayName = "Authenticated user with must_change_password is forbidden on standard endpoints with ProblemDetails")]
    public async Task MustChangePassword_BlocksStandardEndpoints()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(AdminEmail, AdminPassword));
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Access an endpoint not marked with [AllowDuringPasswordChange]
        var response = await _client.GetAsync("/reports/export/funnel");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.ShouldNotBeNull();
        problemDetails.Extensions.ShouldContainKey("code");
        problemDetails.Extensions["code"]?.ToString().ShouldBe("PasswordChangeRequired");
    }

    [Fact(DisplayName = "Changing password clears must_change_password and allows normal API requests")]
    public async Task ChangePassword_ClearsMustChangePassword_AndUnlocks()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(AdminEmail, AdminPassword));
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var newPassword = "New-Correct-Horse-Password-1";
        var changeResponse = await _client.PostAsJsonAsync("/api/auth/change-password", new ChangePasswordRequest(AdminPassword, newPassword));
        changeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var changedDto = await changeResponse.Content.ReadFromJsonAsync<CurrentUserDto>();
        changedDto.ShouldNotBeNull();
        changedDto.MustChangePassword.ShouldBeFalse();

        var meResponse = await _client.GetAsync("/api/auth/me");
        meResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var meDto = await meResponse.Content.ReadFromJsonAsync<CurrentUserDto>();
        meDto.ShouldNotBeNull();
        meDto.MustChangePassword.ShouldBeFalse();
    }

    [Fact(DisplayName = "Logout ends the authenticated session")]
    public async Task Logout_EndsSession()
    {
        await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(AdminEmail, AdminPassword));

        var logoutResponse = await _client.PostAsync("/api/auth/logout", null);
        logoutResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var meResponse = await _client.GetAsync("/api/auth/me");
        meResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
