namespace Simando.Domain.Security;

// Ordered Area < Region < All so PermissionEvaluator can pick the "widest"
// scope across a multi-role user's assignments with a plain comparison.
public enum AccessScope
{
    Area,
    Region,
    All,
}
