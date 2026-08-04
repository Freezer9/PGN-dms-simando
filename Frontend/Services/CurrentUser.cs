using System.Security.Claims;
using Pgn.Dms.Web.Data;

namespace Pgn.Dms.Web.Services;

public static class CurrentUser
{
    public static string DisplayName(ClaimsPrincipal? principal)
    {
        return principal?.Identity?.Name ?? "Guest";
    }

    public static string Initials(ClaimsPrincipal? principal)
    {
        var parts = DisplayName(principal)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return parts.Length switch
        {
            0 => "?",
            1 => parts[0][..1].ToUpperInvariant(),
            _ => $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant()
        };
    }

    public static string PrimaryRole(ClaimsPrincipal? principal)
    {
        if (principal is null) return "Guest";
        return SimandoRoles.All.FirstOrDefault(principal.IsInRole) ?? "Member";
    }

    public static string? UserId(ClaimsPrincipal? principal)
    {
        return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
