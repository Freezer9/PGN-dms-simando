using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Simando.Domain.Geography;
using Simando.Domain.Security;
using Simando.Infrastructure;
using Simando.Infrastructure.Geography;
using Simando.Infrastructure.Persistence;
using Simando.Integration.Tests;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Geography;

// Exercises GeographySeeder against real Postgres -- same Testcontainers
// pattern as AdminSeederTests. Each test gets its own container (xUnit
// default), so the ~92,000-row import runs once or twice per test; batched
// inserts keep that fast enough not to be worth a shared-fixture rewrite.
public class GeographySeederTests : IAsyncLifetime
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

    [Fact(DisplayName = "First run imports all four levels with correct hierarchy and derived type/name")]
    public async Task FirstRun_ImportsAllLevels()
    {
        await using var scope = NewScope();
        var seeder = scope.ServiceProvider.GetRequiredService<GeographySeeder>();
        var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();

        var result = await seeder.SeedAsync();

        result.AlreadySeeded.ShouldBeFalse();
        result.Provinces.ShouldBe(38);
        (await db.Provinces.CountAsync()).ShouldBe(38);
        (await db.Regencies.CountAsync()).ShouldBe(result.Regencies);
        (await db.Districts.CountAsync()).ShouldBe(result.Districts);
        (await db.Villages.CountAsync()).ShouldBe(result.Villages);

        var jatim = await db.Provinces.SingleAsync(p => p.BpsCode == "35");
        jatim.Name.ShouldBe("Jawa Timur");

        // "Kota Surabaya" -> Type=Kota, Name="Surabaya" (prefix stripped).
        var surabaya = await db.Regencies.SingleAsync(r => r.BpsCode == "78" && r.ProvinceId == jatim.Id);
        surabaya.Type.ShouldBe(RegencyType.Kota);
        surabaya.Name.ShouldBe("Surabaya");

        var genteng = await db.Districts.SingleAsync(d => d.RegencyId == surabaya.Id && d.BpsCode == "07");
        genteng.Name.ShouldBe("Genteng");

        // Village code's local segment: '1' prefix -> Kelurahan.
        var kelurahan = await db.Villages.SingleAsync(v => v.DistrictId == genteng.Id && v.BpsCode == "1002");
        kelurahan.Type.ShouldBe(VillageType.Kelurahan);
        kelurahan.Name.ShouldBe("Genteng");
    }

    [Fact(DisplayName = "A second run is idempotent: no duplicate rows, whole import skipped")]
    public async Task SecondRun_IsIdempotent()
    {
        await using (var scope = NewScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<GeographySeeder>();
            var first = await seeder.SeedAsync();
            first.AlreadySeeded.ShouldBeFalse();
        }

        await using (var scope = NewScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<GeographySeeder>();
            var second = await seeder.SeedAsync();
            second.AlreadySeeded.ShouldBeTrue();
        }

        await using var verify = NewScope();
        var db = verify.ServiceProvider.GetRequiredService<SimandoDbContext>();
        (await db.Provinces.CountAsync()).ShouldBe(38);
    }
}
