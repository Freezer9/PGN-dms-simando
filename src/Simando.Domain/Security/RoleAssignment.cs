namespace Simando.Domain.Security;

// Mirrors `user_role_assignment` — docs/design/roles-permissions.md §4
// "Multi-role users". A many-to-many between user and role, each carrying
// its own scope, because one person may hold Sales Area in one Area and
// Reviewer at Region level at the same time.
public sealed class RoleAssignment
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required Role Role { get; init; }
    public Guid? AreaId { get; init; }
    public Guid? RegionId { get; init; }
    public bool Active { get; set; } = true;
    public Guid AssignedBy { get; init; }
    public DateTimeOffset AssignedAt { get; init; }
}
