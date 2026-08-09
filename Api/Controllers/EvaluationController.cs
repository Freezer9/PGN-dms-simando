using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pgn.Dms.Api.Data;
using Pgn.Dms.Api.Services;
using Pgn.Dms.Shared;

namespace Pgn.Dms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EvaluationController(WorkflowService workflow, StageService stages) : ControllerBase
{
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    private string UserName => User.FindFirst(ClaimTypes.Name)?.Value ?? "";

    [HttpGet("{subscriptionId:int}")]
    [Authorize(Roles = "AdminRegional,AreaHead")]
    public async Task<ActionResult<ResumeEvaluasiDto>> Get(int subscriptionId)
    {
        var resume = await workflow.GetResumeAsync(subscriptionId);
        return resume is null ? NotFound() : Ok(resume);
    }

    [HttpPost("{subscriptionId:int}")]
    [Authorize(Roles = "AdminRegional")]
    public async Task<ActionResult<ResumeEvaluasiDto>> Save(int subscriptionId, [FromBody] SaveResumeRequest req)
    {
        var resume = await workflow.SaveResumeAsync(subscriptionId, req.Content, UserId, UserName);
        return Ok(resume);
    }

    /// <summary>Structured stage-7 data. The narrative Resume Evaluasi stays on the endpoints above.</summary>
    [HttpGet("{subscriptionId:int}/detail")]
    public async Task<ActionResult<NolEvaluationDto>> GetDetail(int subscriptionId)
        => await stages.GetEvaluationAsync(subscriptionId) is { } dto ? Ok(dto) : NotFound();

    [HttpPut("{subscriptionId:int}/detail")]
    [Authorize(Roles = "AdminRegional")]
    public async Task<ActionResult<NolEvaluationDto>> SaveDetail(int subscriptionId, [FromBody] NolEvaluationDto req)
        => Ok(await stages.SaveEvaluationAsync(subscriptionId, req, UserId, UserName));
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "AdminRegional,SystemAdmin")]
public class UsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<UserInfo>> GetAll()
    {
        var users = db.Users.Include(u => u.Area).ToList();
        var result = new List<UserInfo>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(new UserInfo
            {
                Id = user.Id, FullName = user.FullName, Email = user.Email ?? "",
                Role = roles.FirstOrDefault() ?? "", AreaId = user.AreaId, AreaName = user.Area?.Name
            });
        }
        return result;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest req)
    {
        var user = new ApplicationUser
        {
            UserName = req.Email, Email = req.Email, EmailConfirmed = true,
            FullName = req.FullName, AreaId = req.AreaId
        };

        if (!SimandoRoles.All.Contains(req.Role))
            return BadRequest($"Role '{req.Role}' tidak dikenali.");

        var result = await userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));

        var roleResult = await userManager.AddToRoleAsync(user, req.Role);
        if (!roleResult.Succeeded)
        {
            // Don't leave a roleless account behind — it would fail every policy check.
            await userManager.DeleteAsync(user);
            return BadRequest(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        return Ok();
    }
}
