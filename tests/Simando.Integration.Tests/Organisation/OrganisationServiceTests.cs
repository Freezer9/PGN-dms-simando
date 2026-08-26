using Microsoft.EntityFrameworkCore;
using Shouldly;
using Simando.Application.Common;
using Simando.Application.Organisation;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Simando.Integration.Tests;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Organisation;

// Region/Area underpin scope resolution for the entire permission model
// (docs/design/roles-permissions.md §1 "Scope") but had no coverage anywhere
// in the suite. OrganisationService is bespoke rather than IEntityService<T>
// (Region/Area aren't AuditableEntity — no soft delete), so the interesting
// behaviour is exactly the FK-restrict-to-EntityInUseException translation
// its own doc comment calls out.
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

    [Fact(DisplayName = "AddRegionAsync then GetRegionsAsync round-trips the new region")]
    public async Task AddRegion_ThenGet_RoundTrips()
    {
        var service = NewService();
        var region = new Region { Id = Guid.NewGuid(), Code = Unique(), Name = "SOR Test", Active = true };

        await service.AddRegionAsync(region);

        (await service.GetRegionsAsync()).ShouldContain(r => r.Id == region.Id && r.Name == "SOR Test");
    }

    [Fact(DisplayName = "AddAreaAsync then GetAreasAsync round-trips the new area under its region")]
    public async Task AddArea_ThenGet_RoundTrips()
    {
        var service = NewService();
        var region = await AddRegionAsync(service);
        var area = new Area { Id = Guid.NewGuid(), RegionId = region.Id, Code = Unique(), Name = "Area Test", Active = true };

        await service.AddAreaAsync(area);

        (await service.GetAreasAsync()).ShouldContain(a => a.Id == area.Id && a.RegionId == region.Id);
    }

    [Fact(DisplayName = "UpdateAreaAsync applies the mutation and persists it")]
    public async Task UpdateArea_PersistsMutation()
    {
        var service = NewService();
        var region = await AddRegionAsync(service);
        var area = new Area { Id = Guid.NewGuid(), RegionId = region.Id, Code = Unique(), Name = "Before", Active = true };
        await service.AddAreaAsync(area);

        await service.UpdateAreaAsync(area.Id, a => a.Name = "After");

        (await service.GetAreasAsync()).Single(a => a.Id == area.Id).Name.ShouldBe("After");
    }

    [Fact(DisplayName = "AddRegionAsync with a Code that already exists throws DuplicateNameException")]
    public async Task AddRegion_DuplicateCode_Throws()
    {
        var service = NewService();
        var code = Unique();
        await service.AddRegionAsync(new Region { Id = Guid.NewGuid(), Code = code, Name = "First", Active = true });

        await Should.ThrowAsync<DuplicateNameException>(() =>
            service.AddRegionAsync(new Region { Id = Guid.NewGuid(), Code = code, Name = "Second", Active = true }));
    }

    [Fact(DisplayName = "DeleteRegionAsync throws EntityInUseException when an Area still references it, and leaves the region intact")]
    public async Task DeleteRegion_StillReferencedByArea_ThrowsAndLeavesRegionIntact()
    {
        var service = NewService();
        var region = await AddRegionAsync(service);
        await service.AddAreaAsync(new Area { Id = Guid.NewGuid(), RegionId = region.Id, Code = Unique(), Name = "Blocking Area", Active = true });

        await Should.ThrowAsync<EntityInUseException>(() => service.DeleteRegionAsync(region.Id));

        (await service.GetRegionsAsync()).ShouldContain(r => r.Id == region.Id);
    }

    [Fact(DisplayName = "DeleteAreaAsync throws EntityInUseException when a Company still references it, and leaves the area intact")]
    public async Task DeleteArea_StillReferencedByCompany_ThrowsAndLeavesAreaIntact()
    {
        var service = NewService();
        var region = await AddRegionAsync(service);
        var area = new Area { Id = Guid.NewGuid(), RegionId = region.Id, Code = Unique(), Name = "Area With Company", Active = true };
        await service.AddAreaAsync(area);
        await SeedCompanyInAreaAsync(area.Id);

        await Should.ThrowAsync<EntityInUseException>(() => service.DeleteAreaAsync(area.Id));

        (await service.GetAreasAsync()).ShouldContain(a => a.Id == area.Id);
    }

    [Fact(DisplayName = "DeleteAreaAsync then DeleteRegionAsync succeed once nothing references them")]
    public async Task DeleteArea_ThenRegion_SucceedWhenUnreferenced()
    {
        var service = NewService();
        var region = await AddRegionAsync(service);
        var area = new Area { Id = Guid.NewGuid(), RegionId = region.Id, Code = Unique(), Name = "Unreferenced Area", Active = true };
        await service.AddAreaAsync(area);

        await service.DeleteAreaAsync(area.Id);
        await service.DeleteRegionAsync(region.Id);

        (await service.GetAreasAsync()).ShouldNotContain(a => a.Id == area.Id);
        (await service.GetRegionsAsync()).ShouldNotContain(r => r.Id == region.Id);
    }

    private async Task<Region> AddRegionAsync(IOrganisationService service)
    {
        var region = new Region { Id = Guid.NewGuid(), Code = Unique(), Name = "Test Region", Active = true };
        await service.AddRegionAsync(region);
        return region;
    }

    private async Task SeedCompanyInAreaAsync(Guid areaId)
    {
        await using var db = NewContext();

        var provinceId = Guid.NewGuid();
        var regencyId = Guid.NewGuid();
        var districtId = Guid.NewGuid();
        var villageId = Guid.NewGuid();
        var industryTypeId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();

        db.Provinces.Add(new Province { Id = provinceId, BpsCode = "11", Name = "Test Province" });
        db.Regencies.Add(new Regency { Id = regencyId, ProvinceId = provinceId, BpsCode = "1101", Type = RegencyType.Kabupaten, Name = "Test Regency" });
        db.Districts.Add(new District { Id = districtId, RegencyId = regencyId, BpsCode = "110101", Name = "Test District" });
        db.Villages.Add(new Village { Id = villageId, DistrictId = districtId, BpsCode = "1101012001", Type = VillageType.Desa, Name = "Test Village" });
        db.IndustryTypes.Add(new IndustryType { Id = industryTypeId, Name = $"Test Industry {Guid.NewGuid():N}" });
        db.Users.Add(new ApplicationUser { Id = creatorId, UserName = Unique(), FullName = "Seed Creator" });

        db.Companies.Add(new Company
        {
            Id = Guid.NewGuid(),
            Nomor = Unique(),
            NamaPerusahaan = "PT Blocking Company",
            VillageId = villageId,
            Alamat = "Jl. Test",
            IndustryTypeId = industryTypeId,
            AreaId = areaId,
            CurrentStage = 1,
            Status = RecordStatus.Draft,
            CreatedBy = creatorId,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync();
    }

    private static string Unique() => Guid.NewGuid().ToString("N")[..8];

    private IOrganisationService NewService() => new OrganisationService(new SingleContextFactory(_container.GetConnectionString()));

    private SimandoDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<SimandoDbContext>()
            .UseNpgsql(_container.GetConnectionString(), npgsql => npgsql.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention()
            .Options;

        return new SimandoDbContext(options, new UnrestrictedCurrentUser());
    }

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
}
