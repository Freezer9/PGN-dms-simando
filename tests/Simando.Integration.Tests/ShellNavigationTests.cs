using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests;

// Sidebar rendering integration tests per docs/design/frontend/01-shell-and-navigation.md.
public class ShellNavigationTests : IAsyncLifetime
{
    private const string Password = "Correct-Horse-Battery-Staple-1";

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

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
        await db.Database.MigrateAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
        (await seeder.SeedAsync("admin", Password, "System Admin", email: "admin@pgn.co.id"))
            .Outcome.ShouldBe(AdminSeedOutcome.Created);

        var regionId = Guid.NewGuid();
        var areaId = Guid.NewGuid();
        db.Regions.Add(new Region { Id = regionId, Code = "SOR-II", Name = "Region II", Active = true });
        db.Areas.Add(new Area { Id = areaId, RegionId = regionId, Code = "SBY", Name = "Area Surabaya", Active = true });
        await db.SaveChangesAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var salesAreaUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "budi.s",
            Email = "budi.s@example.test",
            FullName = "Budi Santoso",
            MustChangePassword = false, // skip the forced-redirect so "/" renders directly
        };
        (await userManager.CreateAsync(salesAreaUser, Password)).Succeeded.ShouldBeTrue();

        db.RoleAssignments.Add(new RoleAssignment
        {
            Id = Guid.NewGuid(),
            UserId = salesAreaUser.Id,
            Role = Role.SalesArea,
            AreaId = areaId,
            RegionId = null,
            Active = true,
            AssignedBy = salesAreaUser.Id,
            AssignedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact(DisplayName = "System Admin's rendered sidebar is its own master-data tree, not the case-work sidebar")]
    public async Task SystemAdmin_SeesAdminTree()
    {
        await SignInAsync("admin@pgn.co.id", Password);
        await ChangePasswordAsync(Password, "New-Correct-Horse-1");

        var html = await GetHtmlAsync("/");

        html.ShouldContain("Organisasi");
        html.ShouldContain(">Pengguna<");
        html.ShouldContain("Referensi");

        // Not just "Beranda" -- that also legitimately appears in the page's
        // own breadcrumb ("Beranda" is Home.razor's own crumb, unrelated to
        // whether the *sidebar* offers it). Direktori/Plotting never appear
        // anywhere except the case-work sidebar section, so their absence is
        // an unambiguous proof that section didn't render for System Admin.
        html.ShouldNotContain(">Direktori<");
        html.ShouldNotContain(">Plotting<");
    }

    [Fact(DisplayName = "Sales Area's rendered sidebar is the case-work list, never the admin tree")]
    public async Task SalesArea_SeesCaseWorkTree()
    {
        await SignInAsync("budi.s@example.test", Password);

        var html = await GetHtmlAsync("/");

        html.ShouldContain(">Beranda<");
        html.ShouldContain(">Direktori<");
        html.ShouldContain(">Plotting<");
        html.ShouldContain(">Peta<");
        html.ShouldContain(">Laporan<");
        html.ShouldNotContain(">Tugas Saya<");
        html.ShouldNotContain("Organisasi");
        html.ShouldNotContain(">Pengguna<");

        // Header -- docs/design/frontend/01-shell-and-navigation.md "Header":
        // "Always show the user's role and scope in the header."
        html.ShouldContain("Sales Area");
        html.ShouldContain("Area Surabaya");
        html.ShouldContain("Budi Santoso");
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

    private async Task<string> GetHtmlAsync(string path)
    {
        var response = await _client.GetAsync(path);
        response.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            $"expected {path} to render, got a redirect to {response.Headers.Location}");

        return await response.Content.ReadAsStringAsync();
    }

    private async Task<string> GetAntiforgeryTokenAsync(string path)
    {
        var response = await _client.GetAsync(path);
        var html = await response.Content.ReadAsStringAsync();

        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        match.Success.ShouldBeTrue($"Could not find the antiforgery token in the {path} page (status {response.StatusCode}):\n{html}");

        return match.Groups[1].Value;
    }
}
