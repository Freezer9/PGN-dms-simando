using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Simando.Domain.MasterData;
using Simando.Domain.Security;
using Simando.Infrastructure;
using Simando.Infrastructure.MasterData;
using Simando.Infrastructure.Persistence;
using Simando.Integration.Tests;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.MasterData;

// Exercises MasterDataSeeder (the literal lookup lists -- units, fuel
// types, industry types, countries, segments) against real Postgres, same
// Testcontainers pattern as AdminSeederTests.
public class MasterDataSeederTests : IAsyncLifetime
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

    [Fact(DisplayName = "First run seeds units, fuel types, industry types, countries and segments")]
    public async Task FirstRun_SeedsAllLists()
    {
        await using var scope = NewScope();
        var seeder = scope.ServiceProvider.GetRequiredService<MasterDataSeeder>();
        var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();

        var result = await seeder.SeedAsync();

        result.Units.ShouldBe(17);
        result.FuelTypes.ShouldBe(13);
        result.IndustryTypes.ShouldBe(20);
        result.Countries.ShouldBe(249);
        result.Segments.ShouldBe(6);

        (await db.UnitsOfMeasure.CountAsync()).ShouldBe(17);
        (await db.UnitSetMembers.CountAsync()).ShouldBe(23);

        // TR backs Capacity, Cooling and EnergyUsage as the same physical unit.
        var tr = await db.UnitsOfMeasure.SingleAsync(u => u.Code == "TR");
        (await db.UnitSetMembers.CountAsync(m => m.UnitId == tr.Id)).ShouldBe(3);

        var segments = await db.Segments.OrderBy(s => s.SortOrder).Select(s => s.Name).ToListAsync();
        segments.ShouldBe(["Bronze 1", "Bronze 2", "Bronze 3", "Silver", "Gold", "Platinum"]);

        (await db.Countries.SingleAsync(c => c.IsoCode == "ID")).Name.ShouldBe("Indonesia");
    }

    [Fact(DisplayName = "A second run is idempotent per table: no duplicate rows")]
    public async Task SecondRun_IsIdempotent()
    {
        await using (var scope = NewScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<MasterDataSeeder>();
            var first = await seeder.SeedAsync();
            first.Countries.ShouldBe(249);
        }

        await using (var scope = NewScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<MasterDataSeeder>();
            var second = await seeder.SeedAsync();
            second.Units.ShouldBe(0);
            second.FuelTypes.ShouldBe(0);
            second.IndustryTypes.ShouldBe(0);
            second.Countries.ShouldBe(0);
            second.Segments.ShouldBe(0);
        }

        await using var verify = NewScope();
        var db = verify.ServiceProvider.GetRequiredService<SimandoDbContext>();
        (await db.Countries.CountAsync()).ShouldBe(249);
        (await db.Segments.CountAsync()).ShouldBe(6);
    }
}
