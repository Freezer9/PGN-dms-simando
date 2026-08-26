using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Simando.Infrastructure.Identity;

// Puts must_change_password on the sign-in cookie itself, so the
// force-redirect-to-/change-password check (docs/build/testing.md A5)
// costs nothing per request — no DB round-trip needed on every navigation.
// SignInManager.RefreshSignInAsync regenerates this claim once the flag
// clears, so it's never stale for longer than one request.
public sealed class ApplicationUserClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<ApplicationUser>(userManager, optionsAccessor)
{
    public const string MustChangePasswordClaimType = "must_change_password";

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        identity.AddClaim(new Claim(MustChangePasswordClaimType, user.MustChangePassword ? "true" : "false"));

        return identity;
    }
}
