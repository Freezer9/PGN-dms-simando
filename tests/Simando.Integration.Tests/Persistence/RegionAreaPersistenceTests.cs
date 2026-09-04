using Microsoft.EntityFrameworkCore;
using Shouldly;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Persistence;

// PostGIS 18 integration tests with container-per-test isolation.
public class RegionAreaPersistenceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("imresamu/postgis:18-3.6-alpine")
        .WithDatabase("simando")
        .WithUsername("simando")
        .WithPassword("simando")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = NewContext();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    [Fact(DisplayName = "Region and Area round-trip through Postgres")]
    public async Task RegionAndArea_RoundTrip()
    {
        var region = new Region { Id = Guid.NewGuid(), Code = "SOR-II", Name = "Region II", Active = true };
        var area = new Area { Id = Guid.NewGuid(), RegionId = region.Id, Code = "SBY", Name = "Area Surabaya", Active = true };

        await using (var db = NewContext())
        {
            db.Regions.Add(region);
            db.Areas.Add(area);
            await db.SaveChangesAsync();
        }

        // Verify round-trip persistence in a fresh context.
        await using var verify = NewContext();
        var storedRegion = await verify.Regions.SingleAsync(r => r.Id == region.Id);
        var storedArea = await verify.Areas.SingleAsync(a => a.Id == area.Id);

        storedRegion.Code.ShouldBe("SOR-II");
        storedRegion.Name.ShouldBe("Region II");
        storedRegion.Active.ShouldBeTrue();
        storedArea.RegionId.ShouldBe(region.Id);
        storedArea.Code.ShouldBe("SBY");
        storedArea.Name.ShouldBe("Area Surabaya");
    }

    [Fact(DisplayName = "Region.Code must be unique")]
    public async Task DuplicateRegionCode_Rejected()
    {
        await using (var db = NewContext())
        {
            db.Regions.Add(new Region { Id = Guid.NewGuid(), Code = "SOR-I", Name = "Region I", Active = true });
            await db.SaveChangesAsync();
        }

        await using var second = NewContext();
        second.Regions.Add(new Region { Id = Guid.NewGuid(), Code = "SOR-I", Name = "Region I duplicate", Active = true });

        await Should.ThrowAsync<DbUpdateException>(() => second.SaveChangesAsync());
    }

    [Fact(DisplayName = "Area.Code is unique within its Region, but the same code is allowed in a different Region")]
    public async Task AreaCode_UniqueWithinRegion_NotGlobally()
    {
        var regionA = new Region { Id = Guid.NewGuid(), Code = "SOR-III", Name = "Region III", Active = true };
        var regionB = new Region { Id = Guid.NewGuid(), Code = "SOR-IV", Name = "Region IV", Active = true };

        await using (var db = NewContext())
        {
            db.Regions.AddRange(regionA, regionB);
            db.Areas.Add(new Area { Id = Guid.NewGuid(), RegionId = regionA.Id, Code = "SBY", Name = "Area Surabaya", Active = true });
            await db.SaveChangesAsync();
        }

        await using (var duplicateInSameRegion = NewContext())
        {
            duplicateInSameRegion.Areas.Add(new Area
            {
                Id = Guid.NewGuid(),
                RegionId = regionA.Id,
                Code = "SBY",
                Name = "Duplicate within Region III",
                Active = true,
            });

            await Should.ThrowAsync<DbUpdateException>(() => duplicateInSameRegion.SaveChangesAsync());
        }

        await using var sameCodeDifferentRegion = NewContext();
        sameCodeDifferentRegion.Areas.Add(new Area
        {
            Id = Guid.NewGuid(),
            RegionId = regionB.Id,
            Code = "SBY",
            Name = "Area Surabaya (Region IV)",
            Active = true,
        });

        await Should.NotThrowAsync(() => sameCodeDifferentRegion.SaveChangesAsync());
    }

    private SimandoDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<SimandoDbContext>()
            .UseNpgsql(_container.GetConnectionString(), npgsql => npgsql.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention()
            .Options;

        return new SimandoDbContext(options, new UnrestrictedCurrentUser());
    }
}
