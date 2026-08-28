using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Api.Security;

namespace Simando.Api.Middleware;

// Middleware enforcing that users with must_change_password flag set can only access
// endpoints marked with [AllowAnonymous] or [AllowDuringPasswordChange].
public sealed class MustChangePasswordMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var isMustChangePassword = context.User.Identity?.IsAuthenticated == true &&
            context.User.HasClaim(SimandoUserClaimsPrincipalFactory.MustChangePasswordClaimType, "true");

        if (isMustChangePassword && !IsExempt(context))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Password Change Required",
                Detail = "You must change your password before accessing this resource.",
                Instance = context.Request.Path
            };
            problemDetails.Extensions["code"] = "PasswordChangeRequired";

            await context.Response.WriteAsJsonAsync(problemDetails);
            return;
        }

        await next(context);
    }

    private static bool IsExempt(HttpContext context)
    {
        var metadata = context.GetEndpoint()?.Metadata;
        if (metadata?.GetMetadata<IAllowAnonymous>() is not null ||
            metadata?.GetMetadata<IAllowDuringPasswordChange>() is not null)
        {
            return true;
        }

        return context.Request.Path.StartsWithSegments("/openapi") ||
               context.Request.Path.StartsWithSegments("/scalar");
    }
}
