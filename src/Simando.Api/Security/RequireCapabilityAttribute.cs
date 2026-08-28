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
}
