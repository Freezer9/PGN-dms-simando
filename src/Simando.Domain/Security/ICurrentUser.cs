namespace Simando.Domain.Security;

// docs/design/roles-permissions.md §4 "Scope resolution", verbatim.
public interface ICurrentUser
{
    Guid UserId { get; }
    AccessScope Scope { get; }
    Guid? AreaId { get; }
    Guid? RegionId { get; }
    bool HasCapability(Capability capability);
    EffectivePermissions Permissions { get; }
    IReadOnlySet<Role> Roles { get; }
    bool IsAuthenticated { get; }
    string FullName { get; }
    string Email { get; }
}
