using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Simando.Application.Security;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Web.Security;

// Circuit-scoped user state. Loaded once via LoadAsync in MainLayout.OnInitializedAsync.
//
// Deliberately does NOT take SimandoDbContext by constructor injection: this
// type implements ICurrentUser, and SimandoDbContext's Company/Plotting/
// CompanyContact query filters need ICurrentUser injected into *their*
// constructor — a DbContext-needs-CurrentUser-needs-DbContext cycle if this
// class also captured a DbContext eagerly. LoadAsync takes the context as a
// parameter instead, supplied by the caller (MainLayout), so construction of
// this object never requires one.
public sealed class CurrentUser(AuthenticationStateProvider authStateProvider) : ICurrentUser
{
    private Task? _loadTask;
    private EffectivePermissions _permissions = new(AccessScope.Area, null, null, new HashSet<Capability>());
    private IReadOnlySet<Role> _roles = new HashSet<Role>();

    public Guid UserId { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string UserName { get; private set; } = string.Empty;

    public string Initials => string.IsNullOrWhiteSpace(FullName)
        ? "U"
        : string.Concat(FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(part => char.ToUpperInvariant(part[0])));

    public IReadOnlyList<RoleAssignmentDisplay> RoleAssignments { get; private set; } = [];

    public IReadOnlySet<Role> Roles => _roles;

    public AccessScope Scope => _permissions.Scope;

    public Guid? AreaId => _permissions.AreaId;

    public Guid? RegionId => _permissions.RegionId;

    public bool HasCapability(Capability capability) => _permissions.HasCapability(capability);

    // Passed straight to NavigationMenuBuilder.Build(permissions, roles) by
    // MainLayout — see Simando.Application.Navigation.
    public EffectivePermissions Permissions => _permissions;

    // Called from both MainLayout and every capability-gated page's
    // OnInitializedAsync, near-simultaneously on first render. A bare
    // "if (_loaded) return" flag (the previous approach) is a race: the
    // second caller would see the flag already set and return immediately
    // with permissions still at their unloaded default (which fails every
    // HasCapability check), before the first caller's query has actually
    // completed. Caching the Task itself, not just a bool, means every
    // caller awaits the *same* in-flight (or completed) load.
    public Task LoadAsync(SimandoDbContext db) => _loadTask ??= LoadCoreAsync(db);

    private async Task LoadCoreAsync(SimandoDbContext db)
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var principal = authState.User;

        if (principal.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var idClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idClaim is null || !Guid.TryParse(idClaim, out var userId))
        {
            return;
        }

        UserId = userId;

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        FullName = user?.FullName ?? principal.Identity.Name ?? string.Empty;
        Email = user?.Email ?? string.Empty;
        UserName = user?.UserName ?? string.Empty;

        var assignments = await db.RoleAssignments.AsNoTracking()
            .Where(a => a.UserId == userId && a.Active)
            .ToListAsync();

        _roles = assignments.Select(a => a.Role).ToHashSet();
        _permissions = PermissionEvaluator.Resolve(assignments);

        RoleAssignments = await BuildRoleAssignmentDisplaysAsync(db, assignments);
    }

    private static async Task<IReadOnlyList<RoleAssignmentDisplay>> BuildRoleAssignmentDisplaysAsync(
        SimandoDbContext db,
        IReadOnlyList<RoleAssignment> assignments)
    {
        var regionIds = assignments.Where(a => a.RegionId.HasValue).Select(a => a.RegionId!.Value).ToHashSet();
        var areaIds = assignments.Where(a => a.AreaId.HasValue).Select(a => a.AreaId!.Value).ToHashSet();

        var regionNames = await db.Regions.AsNoTracking()
            .Where(r => regionIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.Name);

        var areaNames = await db.Areas.AsNoTracking()
            .Where(a => areaIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Name);

        return assignments
            .Select(a => new RoleAssignmentDisplay(a.Role, ScopeLabel(a, regionNames, areaNames)))
            .ToList();
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
