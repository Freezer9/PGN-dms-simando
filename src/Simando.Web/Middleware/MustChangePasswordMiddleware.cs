using Microsoft.AspNetCore.Authorization;
using Simando.Infrastructure.Identity;
using Simando.Web.Security;

namespace Simando.Web.Middleware;

// docs/build/testing.md A5: "must_change_password set -> every route
// redirects to /change-password." must_change_password rides the auth
// cookie itself (ApplicationUserClaimsPrincipalFactory), so this is a claim
// check, not a DB round-trip.
public sealed class MustChangePasswordMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var isMustChangePassword = context.User.Identity?.IsAuthenticated == true &&
            context.User.HasClaim(ApplicationUserClaimsPrincipalFactory.MustChangePasswordClaimType, "true");

        if (isMustChangePassword && !IsExempt(context))
        {
            context.Response.Redirect("/change-password");
            return;
        }

        await next(context);
    }

    // Endpoint-metadata exemption pattern (not hand-listed paths) per
    // docs/build/web-conventions.md#middleware-exemptions-live-at-the-endpoint-not-in-the-middleware.
    private static bool IsExempt(HttpContext context)
    {
        var metadata = context.GetEndpoint()?.Metadata;
        if (metadata?.GetMetadata<IAllowAnonymous>() is not null ||
            metadata?.GetMetadata<IAllowDuringPasswordChange>() is not null)
        {
            return true;
        }

        return context.Request.Path.StartsWithSegments("/_blazor");
    }
}
