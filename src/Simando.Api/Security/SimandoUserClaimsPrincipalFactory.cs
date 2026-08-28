using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Simando.Domain.Security;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;

namespace Simando.Api.Security;

public sealed class SimandoUserClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    IOptions<IdentityOptions> optionsAccessor,
    IDbContextFactory<SimandoDbContext> dbContextFactory)
    : UserClaimsPrincipalFactory<ApplicationUser>(userManager, optionsAccessor)
{
    public const string MustChangePasswordClaimType = "must_change_password";
    public const string ScopeClaimType = "scope";
    public const string AreaIdClaimType = "area_id";
    public const string RegionIdClaimType = "region_id";
    public const string CapabilityClaimType = "capability";
    public const string FullNameClaimType = "full_name";

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        if (!string.IsNullOrWhiteSpace(user.FullName))
        {
            identity.AddClaim(new Claim(FullNameClaimType, user.FullName));
        }

        if (user.MustChangePassword)
        {
            identity.AddClaim(new Claim(MustChangePasswordClaimType, "true"));
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var assignments = await db.RoleAssignments.AsNoTracking()
            .Where(a => a.UserId == user.Id && a.Active)
            .ToListAsync();

        var effectivePermissions = PermissionEvaluator.Resolve(assignments);
        identity.AddClaim(new Claim(ScopeClaimType, effectivePermissions.Scope.ToString()));

        if (effectivePermissions.AreaId.HasValue)
        {
            identity.AddClaim(new Claim(AreaIdClaimType, effectivePermissions.AreaId.Value.ToString()));
        }

        if (effectivePermissions.RegionId.HasValue)
        {
            identity.AddClaim(new Claim(RegionIdClaimType, effectivePermissions.RegionId.Value.ToString()));
        }

        foreach (var assignment in assignments)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, assignment.Role.ToString()));
        }

        foreach (var capability in effectivePermissions.Capabilities)
        {
            identity.AddClaim(new Claim(CapabilityClaimType, capability.ToString()));
        }

        return identity;
    }
}
