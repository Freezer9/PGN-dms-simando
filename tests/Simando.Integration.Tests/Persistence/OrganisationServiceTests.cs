using Microsoft.EntityFrameworkCore;
using Shouldly;
using Simando.Application.Common;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Persistence;

// Region/Area aren't AuditableEntity, so OrganisationService is bespoke
// rather than going through Repository<T>/IEntityService<T> — this exercises
// it directly, same PostGIS container-per-test harness as
// RegionAreaPersistenceTests.cs.
public class OrganisationServiceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgis/postgis:18-3.6-alpine")
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

    [Fact(DisplayName = "AddRegionAsync and AddAreaAsync round-trip")]
    public async Task AddRegionAndArea_RoundTrip()
    {
        var service = NewService();

        var region = new Region { Id = Guid.NewGuid(), Code = "SOR-I", Name = "Region I", Active = true };
        await service.AddRegionAsync(region);

        var area = new Area { Id = Guid.NewGuid(), RegionId = region.Id, Code = "SBY", Name = "Area Surabaya", Active = true };
        await service.AddAreaAsync(area);

        var regions = await service.GetRegionsAsync();
        var areas = await service.GetAreasAsync();

        regions.ShouldContain(r => r.Id == region.Id);
        areas.ShouldContain(a => a.Id == area.Id && a.RegionId == region.Id);
    }

    [Fact(DisplayName = "AddRegionAsync rejects a duplicate Region code")]
    public async Task AddRegion_DuplicateCode_Throws()
    {
        var service = NewService();

        await service.AddRegionAsync(new Region { Id = Guid.NewGuid(), Code = "SOR-II", Name = "Region II", Active = true });

        await Should.ThrowAsync<DuplicateNameException>(() =>
            service.AddRegionAsync(new Region { Id = Guid.NewGuid(), Code = "SOR-II", Name = "Region II duplicate", Active = true }));
    }

    [Fact(DisplayName = "AddAreaAsync rejects a duplicate (Region, Code) pair")]
    public async Task AddArea_DuplicateCodeWithinRegion_Throws()
    {
        var service = NewService();

        var region = new Region { Id = Guid.NewGuid(), Code = "SOR-III", Name = "Region III", Active = true };
        await service.AddRegionAsync(region);
        await service.AddAreaAsync(new Area { Id = Guid.NewGuid(), RegionId = region.Id, Code = "SBY", Name = "Area Surabaya", Active = true });

        await Should.ThrowAsync<DuplicateNameException>(() =>
            service.AddAreaAsync(new Area { Id = Guid.NewGuid(), RegionId = region.Id, Code = "SBY", Name = "Duplicate", Active = true }));
    }

    [Fact(DisplayName = "DeleteRegionAsync is blocked while the Region still has an Area (FK restrict)")]
    public async Task DeleteRegion_WithArea_Throws()
    {
        var service = NewService();

        var region = new Region { Id = Guid.NewGuid(), Code = "SOR-IV", Name = "Region IV", Active = true };
        await service.AddRegionAsync(region);
        await service.AddAreaAsync(new Area { Id = Guid.NewGuid(), RegionId = region.Id, Code = "GRK", Name = "Area Gresik", Active = true });

        await Should.ThrowAsync<EntityInUseException>(() => service.DeleteRegionAsync(region.Id));
    }

    [Fact(DisplayName = "DeleteRegionAsync succeeds for a Region with no Areas")]
    public async Task DeleteRegion_Unreferenced_Succeeds()
    {
        var service = NewService();

        var region = new Region { Id = Guid.NewGuid(), Code = "SOR-V", Name = "Region V", Active = true };
        await service.AddRegionAsync(region);

        await service.DeleteRegionAsync(region.Id);

        var regions = await service.GetRegionsAsync();
        regions.ShouldNotContain(r => r.Id == region.Id);
    }

    [Fact(DisplayName = "UpdateAreaAsync can reassign an Area to a different Region")]
    public async Task UpdateArea_ReassignRegion_Persists()
    {
        var service = NewService();

        var regionA = new Region { Id = Guid.NewGuid(), Code = "SOR-VI", Name = "Region VI", Active = true };
        var regionB = new Region { Id = Guid.NewGuid(), Code = "SOR-VII", Name = "Region VII", Active = true };
        await service.AddRegionAsync(regionA);
        await service.AddRegionAsync(regionB);

        var area = new Area { Id = Guid.NewGuid(), RegionId = regionA.Id, Code = "SDA", Name = "Area Sidoarjo", Active = true };
        await service.AddAreaAsync(area);

        await service.UpdateAreaAsync(area.Id, existing => existing.RegionId = regionB.Id);

        var areas = await service.GetAreasAsync();
        areas.Single(a => a.Id == area.Id).RegionId.ShouldBe(regionB.Id);
    }

    private OrganisationService NewService() => new(new SingleContextFactory(_container.GetConnectionString()));

    private SimandoDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<SimandoDbContext>()
            .UseNpgsql(_container.GetConnectionString(), npgsql => npgsql.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention()
            .Options;

        return new SimandoDbContext(options, new UnrestrictedCurrentUser());
    }

    // OrganisationService takes IDbContextFactory<SimandoDbContext>, same as
    // production DI — this test-only factory hands out a fresh context per
    // call against the shared test container, mirroring AddDbContextFactory.
    private sealed class SingleContextFactory(string connectionString) : IDbContextFactory<SimandoDbContext>
    {
        public SimandoDbContext CreateDbContext() => Build();

        public Task<SimandoDbContext> CreateDbContextAsync(CancellationToken ct = default) => Task.FromResult(Build());

        private SimandoDbContext Build()
        {
            var options = new DbContextOptionsBuilder<SimandoDbContext>()
                .UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite())
                .UseSnakeCaseNamingConvention()
                .Options;

            return new SimandoDbContext(options, new UnrestrictedCurrentUser());
        }
    }

    private sealed class UnrestrictedCurrentUser : ICurrentUser
    {
        public Guid UserId => Guid.Empty;
        public AccessScope Scope => AccessScope.All;
        public Guid? AreaId => null;
        public Guid? RegionId => null;
        public bool HasCapability(Capability capability) => true;
    }
}
