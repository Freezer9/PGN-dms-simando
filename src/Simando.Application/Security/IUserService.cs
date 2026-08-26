using Simando.Domain.Security;

namespace Simando.Application.Security;

// Bespoke, not IEntityService<T> — ApplicationUser/RoleAssignment aren't
// AuditableEntity, and user creation/reset needs UserManager<ApplicationUser>
// for password hashing/validation, not a plain repository.
public interface IUserService
{
    Task<List<UserListItem>> GetUsersAsync(EffectivePermissions actor, CancellationToken ct = default);

    Task<CreateUserResult> CreateUserAsync(
        string fullName,
        string username,
        string? email,
        Role role,
        Guid? areaId,
        Guid? regionId,
        Guid actorUserId,
        EffectivePermissions actor,
        CancellationToken ct = default);

    Task<RoleAssignmentResult> AddRoleAssignmentAsync(
        Guid userId,
        Role role,
        Guid? areaId,
        Guid? regionId,
        Guid actorUserId,
        EffectivePermissions actor,
        CancellationToken ct = default);

    Task DeactivateRoleAssignmentAsync(Guid assignmentId, Guid actorUserId, Guid targetUserId, CancellationToken ct = default);

    // Returns the new one-time password — shown to the admin once, never persisted.
    Task<string> ResetPasswordAsync(Guid userId, CancellationToken ct = default);

    Task SetUserActiveAsync(Guid userId, bool active, CancellationToken ct = default);
}
