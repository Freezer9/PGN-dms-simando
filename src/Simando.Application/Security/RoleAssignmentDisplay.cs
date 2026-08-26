using Simando.Domain.Security;

namespace Simando.Application.Security;

// Shared by CurrentUser (Web, the signed-in user's own header display) and
// IUserService (the Pengguna admin list) — both need the same Role->label
// and scope->label rendering, so it lives here instead of being derived
// twice.
public sealed record RoleAssignmentDisplay(Role Role, string ScopeLabel)
{
    public string RoleLabel => Role switch
    {
        Role.SalesArea => "Sales Area",
        Role.AreaHead => "Area Head",
        Role.RegionalAdmin => "Regional Admin",
        Role.Reviewer => "Reviewer",
        Role.DivisionHead => "Division Head",
        Role.SystemAdmin => "System Admin",
        _ => Role.ToString(),
    };
}
