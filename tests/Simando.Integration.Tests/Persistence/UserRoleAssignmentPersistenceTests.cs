using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Infrastructure;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Simando.Integration.Tests;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Persistence;

// RoleAssignment persistence tests against real PostGIS via AddInfrastructure.
public class UserRoleAssignmentPersistenceTests : IAsyncLifetime
{
    private const string ValidPassword = "Correct-Horse-Battery-Staple-1";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("imresamu/postgis:18-3.6-alpine")
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
                ["Auth:Password:MinLength"] = "12",
                ["Auth:Password:RequireMixed"] = "true",
                ["Auth:Lockout:MaxAttempts"] = "10",
                ["Auth:Lockout:Minutes"] = "15",
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

    [Fact(DisplayName = "A user created through UserManager round-trips with a real hashed password")]
    public async Task User_CreatedThroughUserManager_RoundTrips()
    {
        var userId = Guid.NewGuid();

        await using (var scope = NewScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "budi.s",
                Email = "budi.s@example.test",
                FullName = "Budi S.",
            };

            var result = await userManager.CreateAsync(user, ValidPassword);
            result.Succeeded.ShouldBeTrue(string.Join(';', result.Errors.Select(e => e.Description)));
        }

        await using (var scope = NewScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var stored = await userManager.FindByIdAsync(userId.ToString());

            stored.ShouldNotBeNull();
            stored.FullName.ShouldBe("Budi S.");
            stored.MustChangePassword.ShouldBeTrue();
            stored.Active.ShouldBeTrue();

            (await userManager.CheckPasswordAsync(stored, ValidPassword)).ShouldBeTrue();
            (await userManager.CheckPasswordAsync(stored, "wrong-password")).ShouldBeFalse();
        }
    }

    [Fact(DisplayName = "Duplicate username is rejected by Identity's own unique index")]
    public async Task DuplicateUsername_Rejected()
    {
        await using var scope = NewScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var first = new ApplicationUser { Id = Guid.NewGuid(), UserName = "sari.w", Email = "sari.w@example.test", FullName = "Sari W." };
        (await userManager.CreateAsync(first, ValidPassword)).Succeeded.ShouldBeTrue();

        var second = new ApplicationUser { Id = Guid.NewGuid(), UserName = "sari.w", Email = "sari.w2@example.test", FullName = "Sari W. duplicate" };
        var result = await userManager.CreateAsync(second, ValidPassword);

        result.Succeeded.ShouldBeFalse();
    }

    [Fact(DisplayName = "RoleAssignment persists scoped to an Area, and a Region-scoped row allows a null AreaId")]
    public async Task RoleAssignment_PersistsWithNullableScope()
    {
        var regionId = Guid.NewGuid();
        var areaId = Guid.NewGuid();
        var salesAreaUserId = Guid.NewGuid();
        var reviewerUserId = Guid.NewGuid();
        var areaScopedAssignmentId = Guid.NewGuid();
        var regionScopedAssignmentId = Guid.NewGuid();

        await using (var scope = NewScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            db.Regions.Add(new Region { Id = regionId, Code = "SOR-I", Name = "Region I", Active = true });
            db.Areas.Add(new Area { Id = areaId, RegionId = regionId, Code = "SBY", Name = "Area Surabaya", Active = true });
            await db.SaveChangesAsync();

            var salesArea = new ApplicationUser { Id = salesAreaUserId, UserName = "budi", Email = "budi@example.test", FullName = "Budi" };
            var reviewer = new ApplicationUser { Id = reviewerUserId, UserName = "andi", Email = "andi@example.test", FullName = "Andi" };
            (await userManager.CreateAsync(salesArea, ValidPassword)).Succeeded.ShouldBeTrue();
            (await userManager.CreateAsync(reviewer, ValidPassword)).Succeeded.ShouldBeTrue();

            db.RoleAssignments.Add(new RoleAssignment
            {
                Id = areaScopedAssignmentId,
                UserId = salesAreaUserId,
                Role = Role.SalesArea,
                AreaId = areaId,
                RegionId = null,
                Active = true,
                AssignedBy = salesAreaUserId,
                AssignedAt = DateTimeOffset.UtcNow,
            });
            db.RoleAssignments.Add(new RoleAssignment
            {
                Id = regionScopedAssignmentId,
                UserId = reviewerUserId,
                Role = Role.Reviewer,
                AreaId = null,
                RegionId = regionId,
                Active = true,
                AssignedBy = salesAreaUserId,
                AssignedAt = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        await using var verify = NewScope();
        var verifyDb = verify.ServiceProvider.GetRequiredService<SimandoDbContext>();

        var storedAreaScoped = await verifyDb.RoleAssignments.SingleAsync(ra => ra.Id == areaScopedAssignmentId);
        storedAreaScoped.Role.ShouldBe(Role.SalesArea);
        storedAreaScoped.AreaId.ShouldBe(areaId);
        storedAreaScoped.RegionId.ShouldBeNull();

        var storedRegionScoped = await verifyDb.RoleAssignments.SingleAsync(ra => ra.Id == regionScopedAssignmentId);
        storedRegionScoped.Role.ShouldBe(Role.Reviewer);
        storedRegionScoped.AreaId.ShouldBeNull();
        storedRegionScoped.RegionId.ShouldBe(regionId);
    }
}
