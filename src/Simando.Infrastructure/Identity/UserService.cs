using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Simando.Application.Security;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.Identity;

// UserManager<ApplicationUser> is hard-bound to the directly-scoped
// SimandoDbContext by Identity's own architecture (AddEntityFrameworkStores),
// so this takes the same directly-injected context rather than
// IDbContextFactory — mirrors AdminSeeder.cs exactly, keeps user creation and
// its RoleAssignment write on one connection instead of splitting across two.
internal sealed class UserService(
    SimandoDbContext db,
    UserManager<ApplicationUser> userManager,
    TemporaryPasswordGenerator passwordGenerator) : IUserService
{
    public async Task<List<UserListItem>> GetUsersAsync(EffectivePermissions actor, CancellationToken ct = default)
    {
        var users = await db.Users.AsNoTracking().ToListAsync(ct);
        var assignments = await db.RoleAssignments.AsNoTracking().Where(a => a.Active).ToListAsync(ct);

        if (actor.Scope != AccessScope.All)
        {
            var regionAreaIds = await db.Areas.AsNoTracking()
                .Where(a => a.RegionId == actor.RegionId)
                .Select(a => a.Id)
                .ToListAsync(ct);
            var regionAreaIdSet = regionAreaIds.ToHashSet();

            var inScopeUserIds = assignments
                .Where(a => a.RegionId == actor.RegionId || (a.AreaId is { } areaId && regionAreaIdSet.Contains(areaId)))
                .Select(a => a.UserId)
                .ToHashSet();

            users = users.Where(u => inScopeUserIds.Contains(u.Id)).ToList();
        }

        var regionIds = assignments.Where(a => a.RegionId.HasValue).Select(a => a.RegionId!.Value).ToHashSet();
        var areaIds = assignments.Where(a => a.AreaId.HasValue).Select(a => a.AreaId!.Value).ToHashSet();

        var regionNames = await db.Regions.AsNoTracking()
            .Where(r => regionIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.Name, ct);
        var areaNames = await db.Areas.AsNoTracking()
            .Where(a => areaIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Name, ct);

        var assignmentsByUser = assignments.ToLookup(a => a.UserId);

        return users
            .Select(u =>
            {
                var userAssignments = assignmentsByUser[u.Id].ToList();
                return new UserListItem(
                    u.Id,
                    u.FullName,
                    u.UserName ?? "",
                    u.Email,
                    u.Active,
                    u.LastLoginAt,
                    userAssignments.Select(a => new RoleAssignmentDisplay(a.Role, ScopeLabel(a, regionNames, areaNames))).ToList(),
                    userAssignments.Select(a => a.Id).ToList());
            })
            // Never-logged-in and longest-dormant first — docs/design/roles-permissions.md
            // §5 "surface last_login_at in the user list so dormant accounts are visible"
            // is the whole point of this ordering, not just displaying the column.
            .OrderBy(u => u.LastLoginAt ?? DateTimeOffset.MinValue)
            .ToList();
    }

    public async Task<CreateUserResult> CreateUserAsync(
        string fullName,
        string username,
        string? email,
        Role role,
        Guid? areaId,
        Guid? regionId,
        Guid actorUserId,
        EffectivePermissions actor,
        CancellationToken ct = default)
    {
        if (!PermissionEvaluator.CanAssignRole(actor, role, regionId))
        {
            return CreateUserResult.Failed(["Anda tidak berwenang menetapkan peran ini."]);
        }

        var password = passwordGenerator.Generate();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = username,
            Email = email,
            FullName = fullName,
            MustChangePassword = true,
            Active = true,
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return CreateUserResult.Failed(createResult.Errors.Select(e => e.Description).ToArray());
        }

        db.RoleAssignments.Add(new RoleAssignment
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Role = role,
            AreaId = areaId,
            RegionId = regionId,
            Active = true,
            AssignedBy = actorUserId,
            AssignedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(ct);

        return CreateUserResult.Success(user.Id, password);
    }

    public async Task<RoleAssignmentResult> AddRoleAssignmentAsync(
        Guid userId,
        Role role,
        Guid? areaId,
        Guid? regionId,
        Guid actorUserId,
        EffectivePermissions actor,
        CancellationToken ct = default)
    {
        if (PermissionEvaluator.IsSelfRoleModification(actorUserId, userId))
        {
            return RoleAssignmentResult.Rejected("Tidak dapat mengubah peran sendiri.");
        }

        if (!PermissionEvaluator.CanAssignRole(actor, role, regionId))
        {
            return RoleAssignmentResult.Rejected("Anda tidak berwenang menetapkan peran ini.");
        }

        db.RoleAssignments.Add(new RoleAssignment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Role = role,
            AreaId = areaId,
            RegionId = regionId,
            Active = true,
            AssignedBy = actorUserId,
            AssignedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(ct);

        return RoleAssignmentResult.Success();
    }

    public async Task DeactivateRoleAssignmentAsync(Guid assignmentId, Guid actorUserId, Guid targetUserId, CancellationToken ct = default)
    {
        if (PermissionEvaluator.IsSelfRoleModification(actorUserId, targetUserId))
        {
            throw new InvalidOperationException("Tidak dapat mengubah peran sendiri.");
        }

        var assignment = await db.RoleAssignments.FirstAsync(a => a.Id == assignmentId, ct);
        assignment.Active = false;
        await db.SaveChangesAsync(ct);
    }

    public async Task<string> ResetPasswordAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await db.Users.FirstAsync(u => u.Id == userId, ct);
        var password = passwordGenerator.Generate();

        await userManager.RemovePasswordAsync(user);
        await userManager.AddPasswordAsync(user, password);

        user.MustChangePassword = true;
        await userManager.UpdateAsync(user);

        return password;
    }

    public async Task SetUserActiveAsync(Guid userId, bool active, CancellationToken ct = default)
    {
        var user = await db.Users.FirstAsync(u => u.Id == userId, ct);
        user.Active = active;
        await userManager.UpdateAsync(user);
    }

    private static string ScopeLabel(
        RoleAssignment assignment,
        IReadOnlyDictionary<Guid, string> regionNames,
        IReadOnlyDictionary<Guid, string> areaNames) => assignment switch
        {
            { AreaId: { } areaId } => areaNames.GetValueOrDefault(areaId, "?"),
            { RegionId: { } regionId } => regionNames.GetValueOrDefault(regionId, "?"),
            _ => "Seluruh Region",
        };
}
