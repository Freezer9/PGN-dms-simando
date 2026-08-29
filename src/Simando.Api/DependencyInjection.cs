using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Simando.Api.Security;
using Simando.Domain.Security;
using Simando.Infrastructure.Identity;

namespace Simando.Api;

public static class ApiDependencyInjection
{
    public const string CorsPolicyName = "SimandoSpaCorsPolicy";

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IAuthorizationPolicyProvider, CapabilityAuthorizationPolicyProvider>();
        services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, SimandoUserClaimsPrincipalFactory>();
        services.AddScoped<SignInManager<ApplicationUser>>();
        services.AddScoped<ICurrentUser, ApiCurrentUser>();

        services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ApplicationScheme, options =>
            {
                options.Cookie.Name = "simando_session";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(configuration.GetValue("Auth:SessionTimeoutMinutes", 60));
                options.SlidingExpiration = true;

                // Return 401/403 for API clients instead of redirecting
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

        services.AddAuthorization(options =>
        {
            // Fallback policy: require authentication by default except OpenAPI/Scalar endpoints
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAssertion(context =>
                    context.Resource is HttpContext { Request.Path: var path } &&
                    (path.StartsWithSegments("/openapi") || path.StartsWithSegments("/scalar"))
                        ? true
                        : context.User.Identity?.IsAuthenticated == true)
                .Build();
        });

        // Configure CORS for frontend SPA
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5173", "http://localhost:3000"];

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
