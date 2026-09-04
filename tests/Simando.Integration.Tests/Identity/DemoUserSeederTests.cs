using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Simando.Domain.Security;
using Simando.Infrastructure;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Simando.Integration.Tests;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Identity;

// Exercises DemoUserSeeder directly (not the seed-demo-users CLI wrapper
// around it) against real Postgres — same pattern as AdminSeederTests.
public class DemoUserSeederTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("imresamu/postgis:18-3.6-alpine")
        .WithDatabase("simando")
        .WithUsername("simando")
        .WithPassword("simando")
        .Build();

    private ServiceProvider _serviceProvider = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _container.GetConnectionString(),
                ["Auth:Password:MinLength"] = "12",
                ["Auth:Password:RequireMixed"] = "true",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        services.AddScoped<ICurrentUser, UnrestrictedCurrentUser>();
        _serviceProvider = services.BuildServiceProvider();

        await using var scope = _serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
        await _container.DisposeAsync();
    }

    private AsyncServiceScope NewScope() => _serviceProvider.CreateAsyncScope();

    [Fact(DisplayName = "First run creates the Demo Region/Area and all 5 role accounts")]
    public async Task FirstRun_CreatesRegionAreaAndAccounts()
    {
        await using var scope = NewScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DemoUserSeeder>();
        var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();

        var result = await seeder.SeedAsync("Correct-Horse-Battery-Staple-1");

        result.Errors.ShouldBeEmpty();
        result.Accounts.Count.ShouldBe(5);
        result.Accounts.ShouldAllBe(a => a.WasCreated);

        var region = await db.Regions.SingleAsync(r => r.Code == "DEMO");
        var area = await db.Areas.SingleAsync(a => a.Code == "DEMO");
        area.RegionId.ShouldBe(region.Id);

        var salesArea = await db.Users.SingleAsync(u => u.UserName == "demo.salesarea");
        salesArea.MustChangePassword.ShouldBeFalse();
        salesArea.Active.ShouldBeTrue();
        var salesAreaAssignment = await db.RoleAssignments.SingleAsync(ra => ra.UserId == salesArea.Id);
        salesAreaAssignment.Role.ShouldBe(Role.SalesArea);
        salesAreaAssignment.AreaId.ShouldBe(area.Id);
        salesAreaAssignment.RegionId.ShouldBeNull();

        var regionalAdmin = await db.Users.SingleAsync(u => u.UserName == "demo.regionaladmin");
        var regionalAdminAssignment = await db.RoleAssignments.SingleAsync(ra => ra.UserId == regionalAdmin.Id);
        regionalAdminAssignment.Role.ShouldBe(Role.RegionalAdmin);
        regionalAdminAssignment.RegionId.ShouldBe(region.Id);
        regionalAdminAssignment.AreaId.ShouldBeNull();

        (await db.Users.CountAsync(u => u.UserName!.StartsWith("demo."))).ShouldBe(5);
    }

    [Fact(DisplayName = "A second run is idempotent: no duplicate Region/Area/users")]
    public async Task SecondRun_IsIdempotent()
    {
        await using (var scope = NewScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DemoUserSeeder>();
            var first = await seeder.SeedAsync("Correct-Horse-Battery-Staple-1");
            first.Accounts.ShouldAllBe(a => a.WasCreated);
        }

        await using (var scope = NewScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DemoUserSeeder>();
            var second = await seeder.SeedAsync("Correct-Horse-Battery-Staple-1");
            second.Errors.ShouldBeEmpty();
            second.Accounts.Count.ShouldBe(5);
            second.Accounts.ShouldAllBe(a => !a.WasCreated);
        }

        await using var verify = NewScope();
        var db = verify.ServiceProvider.GetRequiredService<SimandoDbContext>();
        (await db.Regions.CountAsync(r => r.Code == "DEMO")).ShouldBe(1);
        (await db.Areas.CountAsync(a => a.Code == "DEMO")).ShouldBe(1);
        (await db.Users.CountAsync(u => u.UserName!.StartsWith("demo."))).ShouldBe(5);
    }
}
