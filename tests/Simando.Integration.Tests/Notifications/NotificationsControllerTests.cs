using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Simando.Api.Controllers;
using Simando.Application.Notifications;
using Simando.Application.Security;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Notifications;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Notifications;

public class NotificationsControllerTests : IAsyncLifetime
{
    private const string SalesEmail = "sales.notif@pgn.co.id";
    private const string SalesPassword = "SalesPassword123!";

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new(System.Text.Json.JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("imresamu/postgis:18-3.6-alpine")
        .WithDatabase("simando")
        .WithUsername("simando")
        .WithPassword("simando")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private Guid _salesUserId;
    private Guid _notificationId1;
    private Guid _notificationId2;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder
                .ConfigureAppConfiguration((_, config) =>
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Postgres"] = _container.GetConnectionString(),
                        ["Storage:Type"] = "S3",
                        ["Storage:S3:ServiceUrl"] = "http://localhost:9000",
                        ["Storage:S3:Bucket"] = "simando",
                        ["Storage:S3:AccessKey"] = "test",
                        ["Storage:S3:SecretKey"] = "test",
                    }))
                .ConfigureServices(services => services.RemoveAll<IHostedService>()));

        string tempPassword;

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
            await db.Database.MigrateAsync();

            var adminSeeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
            await adminSeeder.SeedAsync("admin", "Admin-Pass-12345!", "System Admin", email: "admin@pgn.co.id");

            var regionId = Guid.NewGuid();
            var areaId = Guid.NewGuid();
            var region = new Region { Id = regionId, Code = "REG-NOTIF", Name = "Region Notif", Active = true };
            var area = new Area { Id = areaId, RegionId = regionId, Code = "AREA-NOTIF", Name = "Area Notif", Active = true };
            var industry = new IndustryType { Id = Guid.NewGuid(), Name = "Industri Notif" };
            var prov = new Province { Id = Guid.NewGuid(), BpsCode = "31", Name = "DKI" };
            var reg = new Regency { Id = Guid.NewGuid(), ProvinceId = prov.Id, BpsCode = "3171", Name = "Jakarta", Type = RegencyType.Kota };
            var dist = new District { Id = Guid.NewGuid(), RegencyId = reg.Id, BpsCode = "317101", Name = "Kecamatan" };
            var vil = new Village { Id = Guid.NewGuid(), DistrictId = dist.Id, BpsCode = "31710101", Name = "Kelurahan", Type = VillageType.Kelurahan };

            db.Regions.Add(region);
            db.Areas.Add(area);
            db.IndustryTypes.Add(industry);
            db.Provinces.Add(prov);
            db.Regencies.Add(reg);
            db.Districts.Add(dist);
            db.Villages.Add(vil);
            await db.SaveChangesAsync();

            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var adminActor = new EffectivePermissions(AccessScope.All, null, null, Enum.GetValues<Capability>().ToHashSet());
            var createResult = await userService.CreateUserAsync(
                "Sales Notif", "sales.notif", SalesEmail,
                Role.SalesArea, areaId, regionId,
                Guid.Empty, adminActor);

            _salesUserId = createResult.UserId;
            tempPassword = createResult.TemporaryPassword!;

            var company = new Company
            {
                Id = Guid.NewGuid(),
                Nomor = "1-31-0001",
                NamaPerusahaan = "PT Notif Test",
                AreaId = areaId,
                IndustryTypeId = industry.Id,
                VillageId = vil.Id,
                Alamat = "Jl Notif",
                CurrentStage = 1,
                Status = RecordStatus.Draft,
                CreatedBy = _salesUserId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            db.Companies.Add(company);

            _notificationId1 = Guid.NewGuid();
            _notificationId2 = Guid.NewGuid();

            db.Notifications.AddRange(
                new Notification
                {
                    Id = _notificationId1,
                    CompanyId = company.Id,
                    RecipientUserId = _salesUserId,
                    Message = "Tugas baru menunggu review Anda",
                    CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
                    ReadAt = null
                },
                new Notification
                {
                    Id = _notificationId2,
                    CompanyId = company.Id,
                    RecipientUserId = _salesUserId,
                    Message = "Pengajuan telah disetujui",
                    CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
                    ReadAt = null
                }
            );

            await db.SaveChangesAsync();
        }

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Authenticate client
        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(SalesEmail, tempPassword));
        loginRes.EnsureSuccessStatusCode();
        var changeRes = await _client.PostAsJsonAsync("/api/auth/change-password", new ChangePasswordRequest(tempPassword, SalesPassword));
        changeRes.EnsureSuccessStatusCode();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact(DisplayName = "Full HTTP lifecycle: UnreadCount -> List -> MarkAsRead -> MarkAllAsRead")]
    public async Task Notification_HttpLifecycle_Succeeds()
    {
        // 1. Initial unread count should be 2
        var countRes = await _client.GetAsync("/api/notifications/unread-count");
        countRes.StatusCode.ShouldBe(HttpStatusCode.OK);
        var countDto = await countRes.Content.ReadFromJsonAsync<UnreadCountDto>(JsonOptions);
        countDto.ShouldNotBeNull();
        countDto.UnreadCount.ShouldBe(2);

        // 2. Fetch notification list
        var listRes = await _client.GetAsync("/api/notifications?limit=10");
        listRes.StatusCode.ShouldBe(HttpStatusCode.OK);
        var list = await listRes.Content.ReadFromJsonAsync<List<NotificationListItem>>(JsonOptions);
        list.ShouldNotBeNull();
        list.Count.ShouldBe(2);
        list[0].CompanyName.ShouldBe("PT Notif Test");
        list[0].CompanyNomor.ShouldBe("1-31-0001");

        // 3. Mark single notification as read
        var markRes = await _client.PostAsync($"/api/notifications/{_notificationId1}/read", null);
        markRes.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify unread count decremented to 1
        var countRes2 = await _client.GetAsync("/api/notifications/unread-count");
        var countDto2 = await countRes2.Content.ReadFromJsonAsync<UnreadCountDto>(JsonOptions);
        countDto2.ShouldNotBeNull();
        countDto2.UnreadCount.ShouldBe(1);

        // 4. Mark all as read
        var markAllRes = await _client.PostAsync("/api/notifications/read-all", null);
        markAllRes.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify unread count cleared to 0
        var countRes3 = await _client.GetAsync("/api/notifications/unread-count");
        var countDto3 = await countRes3.Content.ReadFromJsonAsync<UnreadCountDto>(JsonOptions);
        countDto3.ShouldNotBeNull();
        countDto3.UnreadCount.ShouldBe(0);
    }
}
