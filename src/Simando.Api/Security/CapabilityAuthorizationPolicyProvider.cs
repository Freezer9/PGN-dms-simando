using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Simando.Domain.Security;

namespace Simando.Api.Security;

public sealed class CapabilityAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public const string SinglePolicyPrefix = "Capability_";
    public const string AnyPolicyPrefix = "CapabilityAny_";

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var existingPolicy = await base.GetPolicyAsync(policyName);
        if (existingPolicy != null)
        {
            return existingPolicy;
        }

        if (policyName.StartsWith(SinglePolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var capabilityName = policyName[SinglePolicyPrefix.Length..];
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireAssertion(ctx =>
                    ctx.User.HasClaim(SimandoUserClaimsPrincipalFactory.CapabilityClaimType, capabilityName))
                .Build();
        }

        if (policyName.StartsWith(AnyPolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var capabilities = policyName[AnyPolicyPrefix.Length..].Split('_', StringSplitOptions.RemoveEmptyEntries);
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireAssertion(ctx =>
                    capabilities.Any(c => ctx.User.HasClaim(SimandoUserClaimsPrincipalFactory.CapabilityClaimType, c)))
                .Build();
        }

        return null;
    }
}
