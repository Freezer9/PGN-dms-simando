using Microsoft.AspNetCore.Authorization;
using Simando.Domain.Security;

namespace Simando.Api.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireCapabilityAttribute : AuthorizeAttribute
{
    public RequireCapabilityAttribute(Capability capability)
    {
        Policy = $"Capability_{capability}";
    }

    public RequireCapabilityAttribute(params Capability[] capabilities)
    {
        if (capabilities.Length == 1)
        {
            Policy = $"Capability_{capabilities[0]}";
        }
        else if (capabilities.Length > 1)
        {
            Policy = $"CapabilityAny_{string.Join("_", capabilities)}";
        }
    }
}
