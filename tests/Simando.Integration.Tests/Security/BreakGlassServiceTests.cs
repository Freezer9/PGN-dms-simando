using Microsoft.EntityFrameworkCore;
using Shouldly;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Notifications;
using Simando.Infrastructure.Persistence;
using Simando.Infrastructure.Security;
using Simando.Integration.Tests;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Security;

public class BreakGlassServiceTests : IAsyncLifetime
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

    [Fact(DisplayName = "RequestAccessAsync creates active break-glass session and audit log")]
    public async Task RequestAccessAsync_GrantsAccessAndLogsAudit()
    {
        var (userId, companyId) = await SeedDataAsync();
        var service = new BreakGlassService(new SingleContextFactory(_container.GetConnectionString()), new InAppNotificationChannel(new SingleContextFactory(_container.GetConnectionString())));
        var permissions = new EffectivePermissions(AccessScope.All, null, null, RoleCapabilities.For(Role.SystemAdmin).Capabilities);

        var result = await service.RequestAccessAsync(companyId, "Pemeriksaan Audit Darurat", userId, permissions);

        result.ShouldNotBeNull();
        result.IsActive.ShouldBeTrue();
        result.Reason.ShouldBe("Pemeriksaan Audit Darurat");

        var hasAccess = await service.HasActiveAccessAsync(userId, companyId);
        hasAccess.ShouldBeTrue();

        var logs = await service.GetAuditLogsAsync(permissions);
        logs.Count.ShouldBeGreaterThanOrEqualTo(1);
        logs[0].Reason.ShouldBe("Pemeriksaan Audit Darurat");
    }

    private async Task<(Guid UserId, Guid CompanyId)> SeedDataAsync()
    {
        await using var db = NewContext();

        var provinceId = Guid.NewGuid();
        var regencyId = Guid.NewGuid();
        var districtId = Guid.NewGuid();
        var villageId = Guid.NewGuid();
        var industryTypeId = Guid.NewGuid();
        var regionId = Guid.NewGuid();
        var areaId = Guid.NewGuid();

        db.Provinces.Add(new Province { Id = provinceId, BpsCode = "36", Name = "Banten" });
        db.Regencies.Add(new Regency { Id = regencyId, ProvinceId = provinceId, BpsCode = "3671", Type = RegencyType.Kota, Name = "Tangerang" });
        db.Districts.Add(new District { Id = districtId, RegencyId = regencyId, BpsCode = "367101", Name = "Tangerang" });
        db.Villages.Add(new Village { Id = villageId, DistrictId = districtId, BpsCode = "3671011001", Type = VillageType.Kelurahan, Name = "Sukarasa" });
        db.IndustryTypes.Add(new IndustryType { Id = industryTypeId, Name = "Kimia" });
        db.Regions.Add(new Region { Id = regionId, Code = "SOR1", Name = "SOR 1 Sumatra West Java", Active = true });
        db.Areas.Add(new Area { Id = areaId, RegionId = regionId, Code = "TNG", Name = "Tangerang", Active = true });

        var user = new Simando.Infrastructure.Identity.ApplicationUser { Id = Guid.NewGuid(), UserName = "sysadmin", FullName = "System Administrator" };
        db.Users.Add(user);

        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            NomorSeq = 701,
            Nomor = "0701-36-3671",
            NamaPerusahaan = "PT Tangerang Kimia",
            VillageId = villageId,
            Alamat = "Jl. Merdeka No. 5, Tangerang",
            IndustryTypeId = industryTypeId,
            AreaId = areaId,
            CurrentStage = 2,
            Status = RecordStatus.Draft,
            CreatedBy = user.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Companies.Add(company);

        await db.SaveChangesAsync();
        return (user.Id, companyId);
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
