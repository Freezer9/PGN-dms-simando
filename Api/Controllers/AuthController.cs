using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pgn.Dms.Api.Data;
using Pgn.Dms.Shared;

namespace Pgn.Dms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(UserManager<ApplicationUser> userManager, IConfiguration config) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null) return Unauthorized("Invalid credentials");

        var result = await userManager.CheckPasswordAsync(user, request.Password);
        if (!result) return Unauthorized("Invalid credentials");

        var roles = await userManager.GetRolesAsync(user);
        var area = user.AreaId.HasValue ? (await userManager.Users.Include(u => u.Area).FirstOrDefaultAsync(u => u.Id == user.Id))?.Area?.Name : null;

        var token = GenerateJwt(user, roles, area);

        return Ok(new LoginResponse
        {
            Token = token,
            User = new UserInfo
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                Role = roles.FirstOrDefault() ?? "",
                AreaId = user.AreaId,
                AreaName = area
            }
        });
    }

    private string GenerateJwt(ApplicationUser user, IList<string> roles, string? areaName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? "SIMANDO_SuperSecretKey_2024_MinLength32Chars!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.Name, user.FullName),
            new("area_id", user.AreaId?.ToString() ?? ""),
            new("area_name", areaName ?? "")
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"] ?? "simando",
            audience: config["Jwt:Audience"] ?? "simando",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
