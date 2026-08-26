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

// docs/design/03-roles-permissions.md §2.6 "Bootstrapping the first admin" —
// exercises AdminSeeder directly (not the seed-admin CLI wrapper around it)
// against real Postgres.
public class AdminSeederTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgis/postgis:18-3.6-alpine")
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

    [Fact(DisplayName = "First run creates the seed admin with must_change_password set")]
    public async Task FirstRun_CreatesSeedAdmin()
    {
        await using var scope = NewScope();
        var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
        var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();

        var result = await seeder.SeedAsync("admin", "Correct-Horse-Battery-Staple-1", "System Admin", email: "admin@pgn.co.id");

        result.Outcome.ShouldBe(AdminSeedOutcome.Created);

        var user = await db.Users.SingleAsync(u => u.Email == "admin@pgn.co.id");
        user.UserName.ShouldBe("admin");
        user.MustChangePassword.ShouldBeTrue();
        user.Active.ShouldBeTrue();

        var assignment = await db.RoleAssignments.SingleAsync(ra => ra.UserId == user.Id);
        assignment.Role.ShouldBe(Role.SystemAdmin);
        assignment.Active.ShouldBeTrue();
        assignment.AreaId.ShouldBeNull();
        assignment.RegionId.ShouldBeNull();
    }

    [Fact(DisplayName = "A second run is a no-op once an active System Admin exists")]
    public async Task SecondRun_IsNoOp()
    {
        await using (var scope = NewScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
            var first = await seeder.SeedAsync("admin", "Correct-Horse-Battery-Staple-1", "System Admin", email: "admin@pgn.co.id");
            first.Outcome.ShouldBe(AdminSeedOutcome.Created);
        }

        await using (var scope = NewScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
            var second = await seeder.SeedAsync("admin2", "Correct-Horse-Battery-Staple-2", "Another Admin", email: "admin2@pgn.co.id");
            second.Outcome.ShouldBe(AdminSeedOutcome.AlreadyExists);
        }

        await using var verify = NewScope();
        var db = verify.ServiceProvider.GetRequiredService<SimandoDbContext>();
        (await db.Users.CountAsync()).ShouldBe(1);
    }
}
