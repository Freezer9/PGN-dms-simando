using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests;

// End-to-end HTTP auth pipeline tests per docs/build/03-testing.md §3.
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
                    }))
                // StorageStartupProbe writes to real storage on host start —
                // this test doesn't exercise storage and has no MinIO/S3 of
                // its own, so it would otherwise inherit whatever Storage:S3
                // config happens to be on the machine running the suite.
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

    [Fact(DisplayName = "Unauthenticated request to / redirects to /sign-in (fallback authorization policy)")]
    public async Task Unauthenticated_RedirectsToSignIn()
    {
        var response = await _client.GetAsync("/");

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        RedirectPath(response).ShouldStartWith("/sign-in");
    }

    [Fact(DisplayName = "A17: /forgot-password does not exist, even once signed in")]
    public async Task ForgotPassword_Is404()
    {
        // Unauthenticated hits the fallback policy first (redirect to
        // /sign-in) same as any other undefined route — not a special case
        // for this one. The meaningful check is that no page exists behind
        // the sign-in wall either.
        await SignInAsync(AdminEmail, AdminPassword);
        await ChangePasswordAsync(AdminPassword, "New-Correct-Horse-1");

        var response = await _client.GetAsync("/forgot-password");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Valid sign-in sets the auth cookie; a MustChangePassword user is redirected from any other path (A5)")]
    public async Task ValidSignIn_ForcesChangePassword()
    {
        var signInResponse = await SignInAsync(AdminEmail, AdminPassword);
        signInResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect);

        var homeResponse = await _client.GetAsync("/");

        homeResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        RedirectPath(homeResponse).ShouldStartWith("/change-password");
    }

    [Fact(DisplayName = "A2/A3: wrong password and unknown email are rejected identically")]
    public async Task WrongPassword_And_UnknownEmail_RejectedTheSameWay()
    {
        var wrongPasswordResponse = await SignInAsync(AdminEmail, "definitely-wrong-Password-1");
        var unknownUserResponse = await SignInAsync("no-such-user@pgn.co.id", "whatever-Password-1");

        wrongPasswordResponse.StatusCode.ShouldBe(unknownUserResponse.StatusCode);
        wrongPasswordResponse.Headers.Location!.OriginalString.ShouldBe(unknownUserResponse.Headers.Location!.OriginalString);
    }

    [Fact(DisplayName = "Changing the password clears must_change_password and unlocks normal navigation")]
    public async Task ChangePassword_ClearsMustChangePassword()
    {
        await SignInAsync(AdminEmail, AdminPassword);

        var changeResponse = await ChangePasswordAsync(AdminPassword, "New-Correct-Horse-1");
        changeResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        changeResponse.Headers.Location!.OriginalString.ShouldBe("/");

        var homeResponse = await _client.GetAsync("/");
        homeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private async Task<HttpResponseMessage> SignInAsync(string email, string password)
    {
        var token = await GetAntiforgeryTokenAsync("/sign-in");

        var form = new Dictionary<string, string>
        {
            ["email"] = email,
            ["password"] = password,
            ["__RequestVerificationToken"] = token,
        };

        return await _client.PostAsync("/account/sign-in", new FormUrlEncodedContent(form));
    }

    private async Task<HttpResponseMessage> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var token = await GetAntiforgeryTokenAsync("/change-password");

        var form = new Dictionary<string, string>
        {
            ["currentPassword"] = currentPassword,
            ["newPassword"] = newPassword,
            ["__RequestVerificationToken"] = token,
        };

        return await _client.PostAsync("/account/change-password", new FormUrlEncodedContent(form));
    }

    private async Task<string> GetAntiforgeryTokenAsync(string path)
    {
        var response = await _client.GetAsync(path);
        var html = await response.Content.ReadAsStringAsync();

        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        match.Success.ShouldBeTrue($"Could not find the antiforgery token in the {path} page (status {response.StatusCode}):\n{html}");

        return match.Groups[1].Value;
    }

    // The cookie auth handler's own challenge redirect (e.g. to
    // LoginPath) is an absolute URI; a plain context.Response.Redirect(...)
    // in this app's own middleware (the must-change-password check) is
    // relative. Uri.PathAndQuery throws on a relative Uri, so normalize both
    // shapes to a path-only string.
    private static string RedirectPath(HttpResponseMessage response)
    {
        var location = response.Headers.Location!;
        return location.IsAbsoluteUri ? location.PathAndQuery : location.OriginalString;
    }
}
