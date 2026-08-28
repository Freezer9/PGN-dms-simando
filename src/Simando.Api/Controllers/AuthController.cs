using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Simando.Api.Security;
using Simando.Domain.Security;
using Simando.Infrastructure.Identity;

namespace Simando.Api.Controllers;

public sealed record LoginRequest(string Username, string Password);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed record CurrentUserDto(
    Guid Id,
    string Username,
    string Email,
    string FullName,
    AccessScope Scope,
    Guid? AreaId,
    Guid? RegionId,
    IReadOnlyCollection<Role> Roles,
    IReadOnlyCollection<Capability> Capabilities,
    bool MustChangePassword);

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IUserClaimsPrincipalFactory<ApplicationUser> claimsPrincipalFactory) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<CurrentUserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status423Locked)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid Credentials",
                Detail = "Username and password are required."
            });
        }

        var user = await userManager.FindByEmailAsync(request.Username)
                   ?? await userManager.FindByNameAsync(request.Username);

        var targetIdentifier = user?.UserName ?? request.Username;

        var result = await signInManager.PasswordSignInAsync(
            targetIdentifier,
            request.Password,
            isPersistent: true,
            lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            return StatusCode(StatusCodes.Status423Locked, new ProblemDetails
            {
                Status = StatusCodes.Status423Locked,
                Title = "Account Locked",
                Detail = "This account is temporarily locked due to multiple failed login attempts. Please try again later."
            });
        }

        if (!result.Succeeded || user is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid Credentials",
                Detail = "Username or password is incorrect."
            });
        }

        var principal = await claimsPrincipalFactory.CreateAsync(user);
        var dto = MapPrincipalToDto(user, principal);

        return Ok(dto);
    }

    [HttpPost("logout")]
    [Authorize]
    [AllowDuringPasswordChange]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [AllowDuringPasswordChange]
    [ProducesResponseType<CurrentUserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idClaim is null || !Guid.TryParse(idClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Unauthorized();
        }

        var dto = MapPrincipalToDto(user, User);
        return Ok(dto);
    }

    [HttpPost("change-password")]
    [Authorize]
    [AllowDuringPasswordChange]
    [ProducesResponseType<CurrentUserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = "Current password and new password are required."
            });
        }

        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idClaim is null || !Guid.TryParse(idClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Unauthorized();
        }

        var changeResult = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changeResult.Succeeded)
        {
            var errors = string.Join("; ", changeResult.Errors.Select(e => e.Description));
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Password Change Failed",
                Detail = errors
            });
        }

        user.MustChangePassword = false;
        await userManager.UpdateAsync(user);
        await signInManager.RefreshSignInAsync(user);

        var principal = await claimsPrincipalFactory.CreateAsync(user);
        var dto = MapPrincipalToDto(user, principal);

        return Ok(dto);
    }

    private static CurrentUserDto MapPrincipalToDto(ApplicationUser user, ClaimsPrincipal principal)
    {
        var scopeStr = principal.FindFirst(SimandoUserClaimsPrincipalFactory.ScopeClaimType)?.Value;
        var scope = Enum.TryParse<AccessScope>(scopeStr, out var s) ? s : AccessScope.Area;
        var areaIdStr = principal.FindFirst(SimandoUserClaimsPrincipalFactory.AreaIdClaimType)?.Value;
        var areaId = Guid.TryParse(areaIdStr, out var a) ? a : (Guid?)null;
        var regionIdStr = principal.FindFirst(SimandoUserClaimsPrincipalFactory.RegionIdClaimType)?.Value;
        var regionId = Guid.TryParse(regionIdStr, out var r) ? r : (Guid?)null;
        var roles = principal.FindAll(ClaimTypes.Role)
            .Select(c => Enum.TryParse<Role>(c.Value, out var role) ? role : (Role?)null)
            .Where(r => r.HasValue)
            .Select(r => r!.Value)
            .ToArray();
        var capabilities = principal.FindAll(SimandoUserClaimsPrincipalFactory.CapabilityClaimType)
            .Select(c => Enum.TryParse<Capability>(c.Value, out var cap) ? cap : (Capability?)null)
            .Where(c => c.HasValue)
            .Select(c => c!.Value)
            .ToArray();

        return new CurrentUserDto(
            user.Id,
            user.UserName ?? principal.Identity?.Name ?? string.Empty,
            user.Email ?? principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            user.FullName ?? principal.FindFirstValue(SimandoUserClaimsPrincipalFactory.FullNameClaimType) ?? string.Empty,
            scope,
            areaId,
            regionId,
            roles,
            capabilities,
            user.MustChangePassword
        );
    }
}
