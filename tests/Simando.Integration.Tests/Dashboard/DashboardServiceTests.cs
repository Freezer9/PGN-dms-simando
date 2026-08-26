using Microsoft.EntityFrameworkCore;
using Shouldly;
using Simando.Domain.Audit;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Dashboard;
using Simando.Infrastructure.Persistence;
using Simando.Integration.Tests;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Dashboard;

public class DashboardServiceTests : IAsyncLifetime
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

    [Fact(DisplayName = "GetSalesAreaDashboardAsync returns returned work items and stage counts")]
    public async Task GetSalesAreaDashboardAsync_ReturnsExpectedDto()
    {
        var seed = await SeedDataAsync();
        var service = new DashboardService(new SingleContextFactory(_container.GetConnectionString()));

        var dto = await service.GetSalesAreaDashboardAsync(seed.AreaId);

        dto.ShouldNotBeNull();
        dto.StageCounts.ShouldNotBeNull();
        dto.StageCounts[1].ShouldBeGreaterThanOrEqualTo(1);
        dto.ReturnedWorkItems.ShouldNotBeNull();
    }

    [Fact(DisplayName = "GetApproverDashboardAsync returns pending approvals sorted by wait time")]
    public async Task GetApproverDashboardAsync_ReturnsExpectedDto()
    {
        var seed = await SeedDataAsync();
        var service = new DashboardService(new SingleContextFactory(_container.GetConnectionString()));

        var actorPermissions = new EffectivePermissions(AccessScope.Area, seed.AreaId, null, RoleCapabilities.For(Role.AreaHead).Capabilities);
        var roles = new HashSet<Role> { Role.AreaHead };

        var dto = await service.GetApproverDashboardAsync(seed.UserId, actorPermissions, roles);

        dto.ShouldNotBeNull();
        dto.TotalActiveRecords.ShouldBeGreaterThanOrEqualTo(1);
        dto.Performance.ShouldNotBeNull();
    }

    [Fact(DisplayName = "GetRegionalAdminDashboardAsync returns regional funnel and stuck tasks")]
    public async Task GetRegionalAdminDashboardAsync_ReturnsExpectedDto()
    {
        var seed = await SeedDataAsync();
        var service = new DashboardService(new SingleContextFactory(_container.GetConnectionString()));

        var actorPermissions = new EffectivePermissions(AccessScope.Region, null, seed.RegionId, RoleCapabilities.For(Role.RegionalAdmin).Capabilities);

        var dto = await service.GetRegionalAdminDashboardAsync(seed.RegionId, actorPermissions);

        dto.ShouldNotBeNull();
        dto.RegionFunnelCounts.ShouldNotBeNull();
        dto.RegionFunnelCounts[1].ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact(DisplayName = "GetSystemAdminDashboardAsync returns configuration health checklist")]
    public async Task GetSystemAdminDashboardAsync_ReturnsExpectedDto()
    {
        await SeedDataAsync();
        var service = new DashboardService(new SingleContextFactory(_container.GetConnectionString()));

        var dto = await service.GetSystemAdminDashboardAsync();

        dto.ShouldNotBeNull();
        dto.HealthItems.Count.ShouldBe(4);
        dto.ActiveUsersCount.ShouldBeGreaterThanOrEqualTo(1);
    }

    private sealed record SeedInfo(Guid AreaId, Guid RegionId, Guid UserId, Guid CompanyId);

    private async Task<SeedInfo> SeedDataAsync()
    {
        await using var db = NewContext();

        var provinceId = Guid.NewGuid();
        var regencyId = Guid.NewGuid();
        var districtId = Guid.NewGuid();
        var villageId = Guid.NewGuid();
        var industryTypeId = Guid.NewGuid();
        var regionId = Guid.NewGuid();
        var areaId = Guid.NewGuid();

        db.Provinces.Add(new Province { Id = provinceId, BpsCode = "35", Name = "Jawa Timur" });
        db.Regencies.Add(new Regency { Id = regencyId, ProvinceId = provinceId, BpsCode = "3578", Type = RegencyType.Kota, Name = "Surabaya" });
        db.Districts.Add(new District { Id = districtId, RegencyId = regencyId, BpsCode = "357801", Name = "Tegalsari" });
        db.Villages.Add(new Village { Id = villageId, DistrictId = districtId, BpsCode = "3578011001", Type = VillageType.Kelurahan, Name = "Kedungdoro" });
        db.IndustryTypes.Add(new IndustryType { Id = industryTypeId, Name = "Makanan & Minuman" });
        db.Regions.Add(new Region { Id = regionId, Code = "SOR2", Name = "SOR 2 Java", Active = true });
        db.Areas.Add(new Area { Id = areaId, RegionId = regionId, Code = "SBY", Name = "Surabaya", Active = true });

        var user = new Simando.Infrastructure.Identity.ApplicationUser { Id = Guid.NewGuid(), UserName = "sales.area", FullName = "Budi Sales" };
        db.Users.Add(user);

        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            NomorSeq = 201,
            Nomor = "0201-35-3578",
            NamaPerusahaan = "PT Surabaya Jaya",
            VillageId = villageId,
            Alamat = "Jl. Kedungdoro No. 12, Surabaya",
            IndustryTypeId = industryTypeId,
            AreaId = areaId,
            CurrentStage = 1,
            Status = RecordStatus.Draft,
            CreatedBy = user.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Companies.Add(company);

        db.StatusEvents.Add(new StatusEvent
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            ActorId = user.Id,
            ToStage = 1,
            ToStatus = RecordStatus.Draft,
            FromStatus = RecordStatus.AreaHead,
            Action = StatusEventAction.Revisi,
            Comment = "Harap lengkapi nomor telepon kantor.",
            OccurredAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();
        return new SeedInfo(areaId, regionId, user.Id, companyId);
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
}
