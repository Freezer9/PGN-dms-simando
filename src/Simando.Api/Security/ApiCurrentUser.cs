using System.Security.Claims;
using Simando.Domain.Security;

namespace Simando.Api.Security;

public sealed class ApiCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid UserId =>
        Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

    public AccessScope Scope =>
        Enum.TryParse<AccessScope>(User?.FindFirstValue(SimandoUserClaimsPrincipalFactory.ScopeClaimType), out var scope)
            ? scope
            : AccessScope.Area;

    public Guid? AreaId =>
        Guid.TryParse(User?.FindFirstValue(SimandoUserClaimsPrincipalFactory.AreaIdClaimType), out var areaId)
            ? areaId
            : null;

    public Guid? RegionId =>
        Guid.TryParse(User?.FindFirstValue(SimandoUserClaimsPrincipalFactory.RegionIdClaimType), out var regionId)
            ? regionId
            : null;

    public bool HasCapability(Capability capability) =>
        User?.HasClaim(SimandoUserClaimsPrincipalFactory.CapabilityClaimType, capability.ToString()) == true;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public string FullName => User?.FindFirstValue(SimandoUserClaimsPrincipalFactory.FullNameClaimType) ?? User?.Identity?.Name ?? string.Empty;

    public string Email => User?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    public string UserName => User?.Identity?.Name ?? string.Empty;

    public bool MustChangePassword =>
        User?.HasClaim(SimandoUserClaimsPrincipalFactory.MustChangePasswordClaimType, "true") == true;

    public IReadOnlySet<Role> Roles =>
        User?.FindAll(ClaimTypes.Role)
            .Select(c => Enum.TryParse<Role>(c.Value, out var r) ? (Role?)r : null)
            .Where(r => r.HasValue)
            .Select(r => r!.Value)
            .ToHashSet() ?? [];

    public IReadOnlySet<Capability> Capabilities =>
        User?.FindAll(SimandoUserClaimsPrincipalFactory.CapabilityClaimType)
            .Select(c => Enum.TryParse<Capability>(c.Value, out var cap) ? (Capability?)cap : null)
            .Where(cap => cap.HasValue)
            .Select(cap => cap!.Value)
            .ToHashSet() ?? [];

    public EffectivePermissions Permissions => new(Scope, AreaId, RegionId, Capabilities);
}
