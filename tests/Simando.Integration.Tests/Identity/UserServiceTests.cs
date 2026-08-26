using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Simando.Application.Security;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Infrastructure;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Simando.Integration.Tests;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Identity;

public class UserServiceTests : IAsyncLifetime
{
    private static readonly EffectivePermissions SystemAdmin =
        new(AccessScope.All, null, null, new HashSet<Capability> { Capability.AssignRoles });

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgis/postgis:18-3.6-alpine")
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

    [Fact(DisplayName = "CreateUserAsync round-trips with the first role assignment")]
    public async Task CreateUser_RoundTrips()
    {
        await using var scope = NewScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserService>();
        var actorId = Guid.NewGuid();

        var result = await service.CreateUserAsync(
            "Budi Santoso", "budi.santoso", "budi@example.test",
            Role.SystemAdmin, null, null,
            actorId, SystemAdmin);

        result.Succeeded.ShouldBeTrue();
        result.TemporaryPassword.ShouldNotBeNullOrEmpty();

        var users = await service.GetUsersAsync(SystemAdmin);
        var created = users.ShouldHaveSingleItem();
        created.FullName.ShouldBe("Budi Santoso");
        created.Roles.ShouldHaveSingleItem().Role.ShouldBe(Role.SystemAdmin);
    }

    [Fact(DisplayName = "CreateUserAsync rejects a duplicate username via Identity's own unique index")]
    public async Task CreateUser_DuplicateUsername_Fails()
    {
        await using var scope = NewScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserService>();
        var actorId = Guid.NewGuid();

        (await service.CreateUserAsync("Sari W.", "sari.w", "sari@example.test", Role.SystemAdmin, null, null, actorId, SystemAdmin))
            .Succeeded.ShouldBeTrue();

        var second = await service.CreateUserAsync("Sari W. duplicate", "sari.w", "sari2@example.test", Role.SystemAdmin, null, null, actorId, SystemAdmin);

        second.Succeeded.ShouldBeFalse();
        second.Errors.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "AddRoleAssignmentAsync rejects a Regional Admin assigning SystemAdmin")]
    public async Task AddRoleAssignment_RegionalAdminAssignsSystemAdmin_Rejected()
    {
        await using var scope = NewScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserService>();
        var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();

        var regionId = Guid.NewGuid();
        db.Regions.Add(new Region { Id = regionId, Code = "SOR-I", Name = "Region I", Active = true });
        await db.SaveChangesAsync();

        var actor = new EffectivePermissions(AccessScope.Region, null, regionId, new HashSet<Capability> { Capability.AssignRoles });
        var actorId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        var result = await service.AddRoleAssignmentAsync(targetId, Role.SystemAdmin, null, regionId, actorId, actor);

        result.Succeeded.ShouldBeFalse();
    }

    [Fact(DisplayName = "AddRoleAssignmentAsync and DeactivateRoleAssignmentAsync reject self-modification")]
    public async Task SelfModification_Rejected()
    {
        await using var scope = NewScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserService>();
        var selfId = Guid.NewGuid();

        var result = await service.AddRoleAssignmentAsync(selfId, Role.SystemAdmin, null, null, selfId, SystemAdmin);
        result.Succeeded.ShouldBeFalse();

        await Should.ThrowAsync<InvalidOperationException>(() =>
            service.DeactivateRoleAssignmentAsync(Guid.NewGuid(), selfId, selfId));
    }

    [Fact(DisplayName = "ResetPasswordAsync issues a new password and forces change on next login")]
    public async Task ResetPassword_ChangesPasswordAndForcesChange()
    {
        await using var scope = NewScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserService>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var created = await service.CreateUserAsync("Dewi K.", "dewi.k", "dewi@example.test", Role.SystemAdmin, null, null, Guid.NewGuid(), SystemAdmin);
        var oldPassword = created.TemporaryPassword!;

        var newPassword = await service.ResetPasswordAsync(created.UserId);

        var user = await userManager.FindByIdAsync(created.UserId.ToString());
        user.ShouldNotBeNull();
        (await userManager.CheckPasswordAsync(user, oldPassword)).ShouldBeFalse();
        (await userManager.CheckPasswordAsync(user, newPassword)).ShouldBeTrue();
        user.MustChangePassword.ShouldBeTrue();
    }
}
