using Microsoft.EntityFrameworkCore;
using Shouldly;
using Simando.Application.Directory;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Directory;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Simando.Integration.Tests;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Directory;

// PostGIS integration tests, container-per-test isolation — same pattern as
// the other integration test files.
public class CompanyServiceTests : IAsyncLifetime
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

    [Fact(DisplayName = "CreateAsync allocates a real NomorSeq and renders Nomor with the Prov/Kab BPS suffix")]
    public async Task CreateAsync_RendersNomorWithBpsSuffix()
    {
        var seed = await SeedAsync();
        var service = NewService();

        var result = await service.CreateAsync(NewRequest(seed), seed.CreatorId, seed.CreatorPermissions);

        result.Nomor.ShouldMatch(@"^\d{7}-11-1101$");

        await using var verify = NewContext();
        var company = await verify.Companies.SingleAsync(c => c.Id == result.CompanyId);
        company.NomorSeq.ShouldBeGreaterThan(0);
        company.Nomor.ShouldBe(result.Nomor);
        company.Status.ShouldBe(RecordStatus.Draft);
        company.CurrentStage.ShouldBe((byte)1);
    }

    [Fact(DisplayName = "CreateAsync: a Sales Area actor's company gets their own AreaId")]
    public async Task CreateAsync_SalesAreaActor_GetsOwnArea()
    {
        var seed = await SeedAsync();
        var service = NewService();

        var result = await service.CreateAsync(NewRequest(seed), seed.CreatorId, seed.CreatorPermissions);

        await using var verify = NewContext();
        var company = await verify.Companies.SingleAsync(c => c.Id == result.CompanyId);
        company.AreaId.ShouldBe(seed.CreatorPermissions.AreaId!.Value);
    }

    [Fact(DisplayName = "GetListAsync: an Area-scoped actor does not see another Area's companies")]
    public async Task GetListAsync_AreaScopedActor_DoesNotSeeOtherArea()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var ownCompany = await service.CreateAsync(NewRequest(seed), seed.CreatorId, seed.CreatorPermissions);

        var otherAreaId = Guid.NewGuid();
        var otherRegionId = Guid.NewGuid();
        await using (var db = NewContext())
        {
            db.Regions.Add(new Region { Id = otherRegionId, Code = Guid.NewGuid().ToString("N")[..8], Name = "Other Region", Active = true });
            db.Areas.Add(new Area { Id = otherAreaId, RegionId = otherRegionId, Code = Guid.NewGuid().ToString("N")[..8], Name = "Other Area", Active = true });
            await db.SaveChangesAsync();
        }

        var otherAreaCompany = new Company
        {
            Id = Guid.NewGuid(),
            Nomor = $"9999999-11-{Guid.NewGuid().ToString("N")[..2]}",
            NamaPerusahaan = "PT Other Area",
            VillageId = seed.VillageId,
            Alamat = "Jl. Lain",
            IndustryTypeId = seed.IndustryTypeId,
            AreaId = otherAreaId,
            CurrentStage = 1,
            Status = RecordStatus.Draft,
            CreatedBy = seed.CreatorId,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        await using (var db = NewContext())
        {
            db.Companies.Add(otherAreaCompany);
            await db.SaveChangesAsync();
        }

        // A context bound to the actual scoped actor, not UnrestrictedCurrentUser —
        // proves Company's own row-level-security query filter, not new logic.
        var scopedService = new CompanyService(new ScopedContextFactory(
            _container.GetConnectionString(),
            new ScopedCurrentUser(seed.CreatorId, AccessScope.Area, seed.CreatorPermissions.AreaId, null)));

        var rows = await scopedService.GetListAsync(new CompanyListFilter());

        rows.ShouldContain(r => r.Id == ownCompany.CompanyId);
        rows.ShouldNotContain(r => r.Id == otherAreaCompany.Id);
    }

    [Fact(DisplayName = "GetListAsync: cascading geography filters narrow correctly")]
    public async Task GetListAsync_GeographyFilters_Narrow()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var created = await service.CreateAsync(NewRequest(seed), seed.CreatorId, seed.CreatorPermissions);

        (await service.GetListAsync(new CompanyListFilter(ProvinceId: seed.ProvinceId)))
            .ShouldContain(r => r.Id == created.CompanyId);
        (await service.GetListAsync(new CompanyListFilter(ProvinceId: Guid.NewGuid())))
            .ShouldNotContain(r => r.Id == created.CompanyId);
        (await service.GetListAsync(new CompanyListFilter(VillageId: seed.VillageId)))
            .ShouldContain(r => r.Id == created.CompanyId);
        (await service.GetListAsync(new CompanyListFilter(VillageId: Guid.NewGuid())))
            .ShouldNotContain(r => r.Id == created.CompanyId);
    }

    [Fact(DisplayName = "GetListAsync: Plotting fields are null before a Plotting row exists, populated after")]
    public async Task GetListAsync_PlottingFields_NullThenPopulated()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var created = await service.CreateAsync(NewRequest(seed), seed.CreatorId, seed.CreatorPermissions);

        var beforePlotting = (await service.GetListAsync(new CompanyListFilter())).Single(r => r.Id == created.CompanyId);
        beforePlotting.SalesUserName.ShouldBeNull();
        beforePlotting.PosisiPelanggan.ShouldBeNull();

        await service.SavePlottingAsync(
            created.CompanyId, new SavePlottingRequest(seed.CreatorId, PosisiPelanggan.JalurExisting, Kawasan.NonKawasanIndustri),
            seed.CreatorId, seed.CreatorPermissions);

        var afterPlotting = (await service.GetListAsync(new CompanyListFilter())).Single(r => r.Id == created.CompanyId);
        afterPlotting.SalesUserName.ShouldBe("creator");
        afterPlotting.PosisiPelanggan.ShouldBe(PosisiPelanggan.JalurExisting);
        afterPlotting.Kawasan.ShouldBe(Kawasan.NonKawasanIndustri);
    }

    [Fact(DisplayName = "GetListAsync: PosisiPelanggan and Kawasan filters narrow correctly")]
    public async Task GetListAsync_PosisiPelangganKawasanFilters_Narrow()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var created = await service.CreateAsync(NewRequest(seed), seed.CreatorId, seed.CreatorPermissions);
        await service.SavePlottingAsync(
            created.CompanyId, new SavePlottingRequest(seed.CreatorId, PosisiPelanggan.JalurExisting, Kawasan.NonKawasanIndustri),
            seed.CreatorId, seed.CreatorPermissions);

        (await service.GetListAsync(new CompanyListFilter(PosisiPelanggan: PosisiPelanggan.JalurExisting)))
            .ShouldContain(r => r.Id == created.CompanyId);
        (await service.GetListAsync(new CompanyListFilter(PosisiPelanggan: PosisiPelanggan.Pengembangan)))
            .ShouldNotContain(r => r.Id == created.CompanyId);
        (await service.GetListAsync(new CompanyListFilter(Kawasan: Kawasan.NonKawasanIndustri)))
            .ShouldContain(r => r.Id == created.CompanyId);
        (await service.GetListAsync(new CompanyListFilter(Kawasan: Kawasan.KawasanIndustri)))
            .ShouldNotContain(r => r.Id == created.CompanyId);
    }

    [Fact(DisplayName = "GetListAsync returns Latitude/Longitude matching what CreateAsync stored")]
    public async Task GetListAsync_ReturnsLatLng()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var created = await service.CreateAsync(NewRequest(seed), seed.CreatorId, seed.CreatorPermissions);

        var row = (await service.GetListAsync(new CompanyListFilter())).Single(r => r.Id == created.CompanyId);
        row.Latitude.ShouldBe(-7.25);
        row.Longitude.ShouldBe(112.75);
    }

    [Fact(DisplayName = "UpdateLocationAsync persists a new pin for a Draft company, rejected once submitted")]
    public async Task UpdateLocationAsync_PersistsOnDraft_RejectedOnceSubmitted()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var created = await service.CreateAsync(NewRequest(seed), seed.CreatorId, seed.CreatorPermissions);

        var result = await service.UpdateLocationAsync(created.CompanyId, -6.9, 107.6, seed.CreatorId, seed.CreatorPermissions);
        result.Succeeded.ShouldBeTrue();

        var row = (await service.GetListAsync(new CompanyListFilter())).Single(r => r.Id == created.CompanyId);
        row.Latitude.ShouldBe(-6.9);
        row.Longitude.ShouldBe(107.6);

        await using (var db = NewContext())
        {
            await db.Companies.Where(c => c.Id == created.CompanyId)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.Status, RecordStatus.AreaHead));
        }

        var rejected = await service.UpdateLocationAsync(created.CompanyId, -6.2, 106.8, seed.CreatorId, seed.CreatorPermissions);
        rejected.Succeeded.ShouldBeFalse();
    }

    [Fact(DisplayName = "SoftDeleteAsync succeeds on a Draft company, is rejected once submitted")]
    public async Task SoftDeleteAsync_DraftSucceeds_SubmittedRejected()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var draft = await service.CreateAsync(NewRequest(seed), seed.CreatorId, seed.CreatorPermissions);

        var draftResult = await service.SoftDeleteAsync(draft.CompanyId, seed.CreatorId);
        draftResult.Succeeded.ShouldBeTrue();

        await using (var verify = NewContext())
        {
            (await verify.Companies.IgnoreQueryFilters().SingleAsync(c => c.Id == draft.CompanyId)).DeletedAt.ShouldNotBeNull();
        }

        var submitted = await service.CreateAsync(NewRequest(seed), seed.CreatorId, seed.CreatorPermissions);
        await using (var db = NewContext())
        {
            await db.Companies.Where(c => c.Id == submitted.CompanyId)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.Status, RecordStatus.AreaHead));
        }

        var submittedResult = await service.SoftDeleteAsync(submitted.CompanyId, seed.CreatorId);
        submittedResult.Succeeded.ShouldBeFalse();
    }

    [Fact(DisplayName = "PromoteToProspekAsync requires complete plotting data before advancing to stage 3")]
    public async Task PromoteToProspekAsync_RequiresPlottingData()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var created = await service.CreateAsync(NewRequest(seed), seed.CreatorId, seed.CreatorPermissions);

        // Before saving plotting (stage 1)
        var res1 = await service.PromoteToProspekAsync(created.CompanyId, seed.CreatorId, seed.CreatorPermissions);
        res1.Succeeded.ShouldBeFalse();

        // Save complete plotting (stage 2)
        await service.SavePlottingAsync(
            created.CompanyId, new SavePlottingRequest(seed.CreatorId, PosisiPelanggan.JalurExisting, Kawasan.NonKawasanIndustri),
            seed.CreatorId, seed.CreatorPermissions);

        // Now promote to stage 3
        var res2 = await service.PromoteToProspekAsync(created.CompanyId, seed.CreatorId, seed.CreatorPermissions);
        res2.Succeeded.ShouldBeTrue();

        await using var verify = NewContext();
        var company = await verify.Companies.SingleAsync(c => c.Id == created.CompanyId);
        company.CurrentStage.ShouldBe((byte)2);
    }

    private static CreateCompanyRequest NewRequest(SeedData seed) => new(
        "PT Test", null, seed.VillageId, "Jl. Test", -7.25, 112.75,
        seed.IndustryTypeId, seed.CreatorPermissions.AreaId!.Value, null, null, null, null);

    private CompanyService NewService() => new(new SingleContextFactory(_container.GetConnectionString()));

    private sealed record SeedData(Guid CreatorId, Guid VillageId, Guid ProvinceId, Guid IndustryTypeId, EffectivePermissions CreatorPermissions);

    private async Task<SeedData> SeedAsync()
    {
        await using var db = NewContext();

        var provinceId = Guid.NewGuid();
        var regencyId = Guid.NewGuid();
        var districtId = Guid.NewGuid();
        var villageId = Guid.NewGuid();
        var industryTypeId = Guid.NewGuid();
        var regionId = Guid.NewGuid();
        var areaId = Guid.NewGuid();

        db.Provinces.Add(new Province { Id = provinceId, BpsCode = "11", Name = "Test Province" });
        db.Regencies.Add(new Regency { Id = regencyId, ProvinceId = provinceId, BpsCode = "1101", Type = RegencyType.Kabupaten, Name = "Test Regency" });
        db.Districts.Add(new District { Id = districtId, RegencyId = regencyId, BpsCode = "110101", Name = "Test District" });
        db.Villages.Add(new Village { Id = villageId, DistrictId = districtId, BpsCode = "1101012001", Type = VillageType.Desa, Name = "Test Village" });
        db.IndustryTypes.Add(new IndustryType { Id = industryTypeId, Name = $"Test Industry {Guid.NewGuid():N}" });
        db.Regions.Add(new Region { Id = regionId, Code = Guid.NewGuid().ToString("N")[..8], Name = "Test Region", Active = true });
        db.Areas.Add(new Area { Id = areaId, RegionId = regionId, Code = Guid.NewGuid().ToString("N")[..8], Name = "Test Area", Active = true });

        var creator = new ApplicationUser { Id = Guid.NewGuid(), UserName = "creator", FullName = "creator" };
        db.Users.Add(creator);

        await db.SaveChangesAsync();

        var permissions = new EffectivePermissions(
            AccessScope.Area, areaId, null, RoleCapabilities.For(Role.SalesArea).Capabilities);

        return new SeedData(creator.Id, villageId, provinceId, industryTypeId, permissions);
    }

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

    private sealed class ScopedContextFactory(string connectionString, ICurrentUser currentUser) : IDbContextFactory<SimandoDbContext>
    {
        public SimandoDbContext CreateDbContext() => Build();

        public Task<SimandoDbContext> CreateDbContextAsync(CancellationToken ct = default) => Task.FromResult(Build());

        private SimandoDbContext Build()
        {
            var options = new DbContextOptionsBuilder<SimandoDbContext>()
                .UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite())
                .UseSnakeCaseNamingConvention()
                .Options;

            return new SimandoDbContext(options, currentUser);
        }
    }

    private sealed class ScopedCurrentUser(Guid userId, AccessScope scope, Guid? areaId, Guid? regionId) : ICurrentUser
    {
        public Guid UserId => userId;
        public AccessScope Scope => scope;
        public Guid? AreaId => areaId;
        public Guid? RegionId => regionId;
        public bool HasCapability(Capability capability) => true;
        public EffectivePermissions Permissions => new(scope, areaId, regionId, Enum.GetValues<Capability>().ToHashSet());
        public IReadOnlySet<Role> Roles => new HashSet<Role> { Role.SystemAdmin };
        public bool IsAuthenticated => true;
        public string FullName => "Scoped User";
        public string Email => "user@pgn.co.id";
    }
}
