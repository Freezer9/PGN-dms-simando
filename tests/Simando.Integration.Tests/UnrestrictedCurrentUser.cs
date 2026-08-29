using Simando.Domain.Security;

namespace Simando.Integration.Tests;

// SimandoDbContext's constructor requires ICurrentUser (its Company/Plotting/
// CompanyContact query filters need it), but ICurrentUser is only registered
// by Simando.Web's Program.cs (CurrentUser : ICurrentUser), not by
// AddInfrastructure. Tests that build a ServiceCollection from
// AddInfrastructure alone (not a full WebApplicationFactory host) need this
// stub registered so SimandoDbContext can resolve via DI at all.
internal sealed class UnrestrictedCurrentUser : ICurrentUser
{
    public Guid UserId => Guid.Empty;
    public AccessScope Scope => AccessScope.All;
    public Guid? AreaId => null;
    public Guid? RegionId => null;
    public bool HasCapability(Capability capability) => true;
    public EffectivePermissions Permissions => new(AccessScope.All, null, null, Enum.GetValues<Capability>().ToHashSet());
    public IReadOnlySet<Role> Roles => new HashSet<Role> { Role.SystemAdmin };
    public bool IsAuthenticated => true;
    public string FullName => "System Administrator";
    public string Email => "admin@pgn.co.id";
}
