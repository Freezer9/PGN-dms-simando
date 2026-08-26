namespace Simando.Application.Security;

// What Users.razor renders one row from — RoleAssignmentIds line up
// positionally with Roles so the page can call DeactivateRoleAssignmentAsync
// on a specific row without a second lookup.
public sealed record UserListItem(
    Guid Id,
    string FullName,
    string UserName,
    string? Email,
    bool Active,
    DateTimeOffset? LastLoginAt,
    IReadOnlyList<RoleAssignmentDisplay> Roles,
    IReadOnlyList<Guid> RoleAssignmentIds);
