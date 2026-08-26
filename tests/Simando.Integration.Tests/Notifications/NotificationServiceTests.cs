using Microsoft.EntityFrameworkCore;
using Shouldly;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Notifications;
using Simando.Domain.Organisation;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Notifications;
using Simando.Infrastructure.Persistence;
using Simando.Integration.Tests;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Notifications;

public class NotificationServiceTests : IAsyncLifetime
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

    [Fact(DisplayName = "GetUnreadCountAsync returns correct unread count")]
    public async Task GetUnreadCountAsync_ReturnsCount()
    {
        var (userId, companyId) = await SeedDataAsync();
        var service = new NotificationService(new SingleContextFactory(_container.GetConnectionString()));

        var count = await service.GetUnreadCountAsync(userId);
        count.ShouldBe(2);
    }

    [Fact(DisplayName = "GetNotificationsAsync returns notification list with company info")]
    public async Task GetNotificationsAsync_ReturnsList()
    {
        var (userId, companyId) = await SeedDataAsync();
        var service = new NotificationService(new SingleContextFactory(_container.GetConnectionString()));

        var list = await service.GetNotificationsAsync(userId);

        list.Count.ShouldBe(2);
        list[0].CompanyName.ShouldBe("PT Madura Energy");
        list[0].Message.ShouldContain("memerlukan persetujuan");
    }

    [Fact(DisplayName = "MarkAsReadAsync updates ReadAt timestamp")]
    public async Task MarkAsReadAsync_UpdatesReadAt()
    {
        var (userId, companyId) = await SeedDataAsync();
        var service = new NotificationService(new SingleContextFactory(_container.GetConnectionString()));

        var list = await service.GetNotificationsAsync(userId);
        var unreadId = list[0].Id;

        await service.MarkAsReadAsync(unreadId, userId);

        var newCount = await service.GetUnreadCountAsync(userId);
        newCount.ShouldBe(1);
    }

    [Fact(DisplayName = "MarkAllAsReadAsync marks all user notifications read")]
    public async Task MarkAllAsReadAsync_MarksAllRead()
    {
        var (userId, companyId) = await SeedDataAsync();
        var service = new NotificationService(new SingleContextFactory(_container.GetConnectionString()));

        await service.MarkAllAsReadAsync(userId);

        var count = await service.GetUnreadCountAsync(userId);
        count.ShouldBe(0);
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

        db.Provinces.Add(new Province { Id = provinceId, BpsCode = "35", Name = "Jawa Timur" });
        db.Regencies.Add(new Regency { Id = regencyId, ProvinceId = provinceId, BpsCode = "3527", Type = RegencyType.Kabupaten, Name = "Sampang" });
        db.Districts.Add(new District { Id = districtId, RegencyId = regencyId, BpsCode = "352701", Name = "Sampang" });
        db.Villages.Add(new Village { Id = villageId, DistrictId = districtId, BpsCode = "3527011001", Type = VillageType.Desa, Name = "Polagan" });
        db.IndustryTypes.Add(new IndustryType { Id = industryTypeId, Name = "Pengolahan Mineral" });
        db.Regions.Add(new Region { Id = regionId, Code = "SOR2", Name = "SOR 2 Java", Active = true });
        db.Areas.Add(new Area { Id = areaId, RegionId = regionId, Code = "SUB", Name = "Surabaya", Active = true });

        var user = new Simando.Infrastructure.Identity.ApplicationUser { Id = Guid.NewGuid(), UserName = "area.head", FullName = "Area Head User" };
        db.Users.Add(user);

        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            NomorSeq = 301,
            Nomor = "0301-35-3527",
            NamaPerusahaan = "PT Madura Energy",
            VillageId = villageId,
            Alamat = "Jl. Raya Madura No. 88, Sampang",
            IndustryTypeId = industryTypeId,
            AreaId = areaId,
            CurrentStage = 6,
            Status = RecordStatus.AreaHead,
            CreatedBy = user.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Companies.Add(company);

        db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = user.Id,
            CompanyId = companyId,
            Message = "Berkas PT Madura Energy memerlukan persetujuan Anda.",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-30),
            ReadAt = null
        });

        db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = user.Id,
            CompanyId = companyId,
            Message = "Dokumen A1 Registrasi telah diunggah.",
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-2),
            ReadAt = null
        });

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
