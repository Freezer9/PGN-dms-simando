using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Simando.Infrastructure.Identity;
using Simando.Web.Security;

namespace Simando.Web.Controllers;

// HTTP auth controller per docs/build/web-conventions.md.
[ApiController]
[Route("account")]
public sealed class AccountController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IAntiforgery antiforgery) : ControllerBase
{
    [HttpPost("sign-in")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromForm] string email, [FromForm] string password)
    {
        if (!await ValidateAntiforgeryAsync())
        {
            return Redirect("/sign-in?error=1");
        }

        var user = await userManager.FindByEmailAsync(email);
        var targetIdentifier = user?.UserName ?? email;

        var result = await signInManager.PasswordSignInAsync(targetIdentifier, password, isPersistent: true, lockoutOnFailure: true);

        // Anti-enumeration: identical response for invalid user/password (docs/build/testing.md A2/A3).
        return result.Succeeded ? Redirect("/") : Redirect("/sign-in?error=1");
    }

    [HttpPost("change-password")]
    [Authorize]
    [AllowDuringPasswordChange]
    public async Task<IActionResult> ChangePassword([FromForm] string currentPassword, [FromForm] string newPassword)
    {
        if (!await ValidateAntiforgeryAsync())
        {
            return Redirect("/change-password?error=1");
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Redirect("/sign-in");
        }

        var changeResult = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!changeResult.Succeeded)
        {
            return Redirect("/change-password?error=1");
        }

        user.MustChangePassword = false;
        await userManager.UpdateAsync(user);

        // Refresh principal claims to clear must_change_password claim immediately.
        await signInManager.RefreshSignInAsync(user);

        return Redirect("/");
    }

    [HttpGet("sign-out")]
    [Authorize]
    [AllowDuringPasswordChange]
    public async Task<IActionResult> SignOutAsync()
    {
        await signInManager.SignOutAsync();
        return Redirect("/sign-in");
    }

    private async Task<bool> ValidateAntiforgeryAsync()
    {
        try
        {
            await antiforgery.ValidateRequestAsync(HttpContext);
            return true;
        }
        catch (AntiforgeryValidationException)
        {
            return false;
        }
    }
}
