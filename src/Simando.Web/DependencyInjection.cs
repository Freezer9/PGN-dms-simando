using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Simando.Domain.Security;
using Simando.Infrastructure.Identity;
using Simando.Web.Components.Layout;
using Simando.Web.Security;

namespace Simando.Web;

public static class DependencyInjection
{
    // ASP.NET Core cookie auth & SignInManager registration (requires Microsoft.AspNetCore.App).
    public static IServiceCollection AddWebIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<SignInManager<ApplicationUser>>();

        services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ApplicationScheme, options =>
            {
                options.LoginPath = "/sign-in";
                options.AccessDeniedPath = "/access-denied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(configuration.GetValue("Auth:SessionTimeoutMinutes", 60));
                options.SlidingExpiration = true;
            });

        // Secure-by-default FallbackPolicy. RequireAssertion exempts /_blazor and /_framework without UseWhen pipeline branching.
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAssertion(context =>
                    context.Resource is HttpContext { Request.Path: var path } &&
                    (path.StartsWithSegments("/_blazor") || path.StartsWithSegments("/_framework"))
                        ? true
                        : context.User.Identity?.IsAuthenticated == true)
                .Build();
        });

        services.AddCascadingAuthenticationState();

        return services;
    }

    // Circuit-scoped CurrentUser & BreadcrumbState for MainLayout.
    //
    // ICurrentUser resolves to the same scoped CurrentUser instance — it's
    // also consumed by SimandoDbContext's row-level-security query filters
    // (see CompanyConfiguration). CurrentUser itself takes no DbContext
    // dependency (see its own comment), so this registration introduces no
    // construction-order cycle.
    public static IServiceCollection AddShellServices(this IServiceCollection services)
    {
        services.AddScoped<CurrentUser>();
        services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<CurrentUser>());
        services.AddScoped<BreadcrumbState>();

        return services;
    }
}
