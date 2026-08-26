namespace Simando.Domain.Security;

public sealed record EffectivePermissions(
    AccessScope Scope,
    Guid? AreaId,
    Guid? RegionId,
    IReadOnlySet<Capability> Capabilities)
{
    public bool HasCapability(Capability capability) => Capabilities.Contains(capability);
}

// docs/design/roles-permissions.md §1: an action is permitted only when
// scope, capability and turn all hold. Kept as plain static functions over
// value types — no I/O, no EF Core — so the rules run and are demonstrable
// without a database, per AGENTS.md.
public static class PermissionEvaluator
{
    public static EffectivePermissions Resolve(IEnumerable<RoleAssignment> assignments)
    {
        var capabilities = new HashSet<Capability>();
        var scope = AccessScope.Area;
        Guid? areaId = null;
        Guid? regionId = null;

        foreach (var assignment in assignments.Where(a => a.Active))
        {
            var profile = RoleCapabilities.For(assignment.Role);
            capabilities.UnionWith(profile.Capabilities);

            if (profile.DefaultScope > scope)
            {
                scope = profile.DefaultScope;
            }

            // "Widest of the active assignments" — §4. Where a user holds more
            // than one Region-scoped (or Area-scoped) assignment naming different
            // regions/areas, the source doc doesn't specify how to combine them;
            // the first one encountered wins rather than inventing a model.
            if (profile.DefaultScope == AccessScope.Area)
            {
                areaId ??= assignment.AreaId;
            }

            if (profile.DefaultScope == AccessScope.Region)
            {
                regionId ??= assignment.RegionId;
            }
        }

        return new EffectivePermissions(scope, areaId, regionId, capabilities);
    }

    // The row-level-security check — docs/build/architecture.md#row-level-security.
    public static bool CanView(
        AccessScope scope,
        Guid? userAreaId,
        Guid? userRegionId,
        Guid recordAreaId,
        Guid recordRegionId) => scope switch
        {
            AccessScope.All => true,
            AccessScope.Region => userRegionId == recordRegionId,
            AccessScope.Area => userAreaId == recordAreaId,
            _ => false,
        };

    // Combines the scope check with the view capability itself, so a role
    // whose "View company records" cell is 🔓 (break-glass only, e.g. System
    // Admin) is correctly denied even though its scope is All.
    public static bool CanViewRecord(EffectivePermissions permissions, Guid recordAreaId, Guid recordRegionId) =>
        permissions.HasCapability(Capability.ViewCompanyRecords) &&
        CanView(permissions.Scope, permissions.AreaId, permissions.RegionId, recordAreaId, recordRegionId);

    // "Visible ≠ actionable" — §1. A role can hold a capability and still be
    // blocked because the record isn't at their step.
    public static bool CanAct(EffectivePermissions permissions, Capability capability, bool isUsersTurn) =>
        permissions.HasCapability(capability) && isUsersTurn;

    // Segregation of duties — §4: a user may never act on an approval step
    // for a record they created, or one they edited at stage 7.
    public static bool IsSelfApproval(Guid actorId, Guid creatorId, IEnumerable<Guid> stage7EditorIds) =>
        actorId == creatorId || stage7EditorIds.Contains(actorId);

    // "Nobody may modify their own roles" — §5 guard rails.
    public static bool IsSelfRoleModification(Guid actorId, Guid targetUserId) =>
        actorId == targetUserId;

    private static readonly IReadOnlySet<Role> RegionalAdminAssignableRoles =
        new HashSet<Role> { Role.SalesArea, Role.AreaHead, Role.Reviewer };

    // "Regional Admin may only assign Sales Area / Area Head / Reviewer, and
    // only within its own Region" — §5 "Who assigns roles". Closes the gap
    // RoleCapabilities.cs's own comment flags as a caller-side check that
    // needs the target role and region, not a capability distinction.
    public static bool CanAssignRole(EffectivePermissions actor, Role targetRole, Guid? targetRegionId) =>
        actor.Scope switch
        {
            AccessScope.All => true,
            AccessScope.Region => RegionalAdminAssignableRoles.Contains(targetRole) && targetRegionId == actor.RegionId,
            _ => false,
        };
}
