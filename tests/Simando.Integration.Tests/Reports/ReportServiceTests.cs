using Microsoft.EntityFrameworkCore;
using Shouldly;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Nol;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Domain.Survey;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Persistence;
using Simando.Infrastructure.Reports;
using Simando.Integration.Tests;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Reports;

public class ReportServiceTests : IAsyncLifetime
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

    [Fact(DisplayName = "GetFunnelAsync returns funnel stage metrics")]
    public async Task GetFunnelAsync_ReturnsExpectedDto()
    {
        var seed = await SeedDataAsync();
        var service = new ReportsService(new SingleContextFactory(_container.GetConnectionString()));
        var permissions = new EffectivePermissions(AccessScope.All, null, null, RoleCapabilities.For(Role.SystemAdmin).Capabilities);

        var dto = await service.GetFunnelAsync(permissions);

        dto.ShouldNotBeNull();
        dto.TotalRecords.ShouldBeGreaterThanOrEqualTo(1);
        dto.Stages.Count.ShouldBe(8);
    }

    [Fact(DisplayName = "GetGasDemandAsync returns gas demand aggregated by stage, region, industry")]
    public async Task GetGasDemandAsync_ReturnsExpectedDto()
    {
        var seed = await SeedDataAsync();
        var service = new ReportsService(new SingleContextFactory(_container.GetConnectionString()));
        var permissions = new EffectivePermissions(AccessScope.All, null, null, RoleCapabilities.For(Role.SystemAdmin).Capabilities);

        var dto = await service.GetGasDemandAsync(permissions);

        dto.ShouldNotBeNull();
        dto.ByStage.Count.ShouldBe(8);
        dto.GrandTotalDemandMMBtu.ShouldBeGreaterThanOrEqualTo(0m);
    }

    [Fact(DisplayName = "GetSurveyProductivityAsync returns productivity metrics")]
    public async Task GetSurveyProductivityAsync_ReturnsExpectedDto()
    {
        await SeedDataAsync();
        var service = new ReportsService(new SingleContextFactory(_container.GetConnectionString()));
        var permissions = new EffectivePermissions(AccessScope.All, null, null, RoleCapabilities.For(Role.SystemAdmin).Capabilities);

        var dto = await service.GetSurveyProductivityAsync(permissions);

        dto.ShouldNotBeNull();
        dto.Rows.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "GetNolOutcomesAsync returns outcome statistics")]
    public async Task GetNolOutcomesAsync_ReturnsExpectedDto()
    {
        await SeedDataAsync();
        var service = new ReportsService(new SingleContextFactory(_container.GetConnectionString()));
        var permissions = new EffectivePermissions(AccessScope.All, null, null, RoleCapabilities.For(Role.SystemAdmin).Capabilities);

        var dto = await service.GetNolOutcomesAsync(permissions);

        dto.ShouldNotBeNull();
        dto.RejectionReasons.ShouldNotBeEmpty();
    }

    private async Task<Guid> SeedDataAsync()
    {
        await using var db = NewContext();

        var provinceId = Guid.NewGuid();
        var regencyId = Guid.NewGuid();
        var districtId = Guid.NewGuid();
        var villageId = Guid.NewGuid();
        var industryTypeId = Guid.NewGuid();
        var regionId = Guid.NewGuid();
        var areaId = Guid.NewGuid();

        db.Provinces.Add(new Province { Id = provinceId, BpsCode = "31", Name = "DKI Jakarta" });
        db.Regencies.Add(new Regency { Id = regencyId, ProvinceId = provinceId, BpsCode = "3171", Type = RegencyType.Kota, Name = "Jakarta Selatan" });
        db.Districts.Add(new District { Id = districtId, RegencyId = regencyId, BpsCode = "317101", Name = "Kebayoran Baru" });
        db.Villages.Add(new Village { Id = villageId, DistrictId = districtId, BpsCode = "3171011001", Type = VillageType.Kelurahan, Name = "Senayan" });
        db.IndustryTypes.Add(new IndustryType { Id = industryTypeId, Name = "Komersial & Perhotelan" });
        db.Regions.Add(new Region { Id = regionId, Code = "SOR1", Name = "SOR 1 Sumatra West Java", Active = true });
        db.Areas.Add(new Area { Id = areaId, RegionId = regionId, Code = "JKT", Name = "Jakarta", Active = true });

        var user = new Simando.Infrastructure.Identity.ApplicationUser { Id = Guid.NewGuid(), UserName = "sales.jkt", FullName = "Sales Jakarta" };
        db.Users.Add(user);

        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            NomorSeq = 501,
            Nomor = "0501-31-3171",
            NamaPerusahaan = "PT Hotel Plaza Jakarta",
            VillageId = villageId,
            Alamat = "Jl. Jendral Sudirman No. 1, Jakarta",
            IndustryTypeId = industryTypeId,
            AreaId = areaId,
            CurrentStage = 4,
            Status = RecordStatus.Draft,
            CreatedBy = user.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Companies.Add(company);

        db.Surveys.Add(new Survey
        {
            CompanyId = companyId,
            TanggalSurvey = DateOnly.FromDateTime(DateTime.UtcNow),
            JumlahKebutuhanEnergi = 250.00m
        });

        await db.SaveChangesAsync();
        return companyId;
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
