using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pgn.Dms.Api.Services;
using Pgn.Dms.Shared;

namespace Pgn.Dms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController(SubscriptionService subs, WorkflowService workflow) : ControllerBase
{
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    private string UserName => User.FindFirst(ClaimTypes.Name)?.Value ?? "";

    [HttpGet]
    public async Task<List<SubscriptionDto>> GetAll() => await subs.GetAllAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SubscriptionDto>> Get(int id)
    {
        var sub = await subs.GetAsync(id);
        return sub is null ? NotFound() : Ok(sub);
    }

    [HttpPost]
    [Authorize(Roles = "SalesArea")]
    public async Task<ActionResult<SubscriptionDto>> Create([FromBody] CreateSubscriptionRequest req)
    {
        var sub = await subs.CreateAsync(req, UserId, UserName);
        return CreatedAtAction(nameof(Get), new { id = sub.Id }, sub);
    }

    [HttpPost("{id:int}/upload")]
    [Authorize(Roles = "SalesArea")]
    public async Task<ActionResult<SubmissionRecordDto>> Upload(int id, IFormFile file)
    {
        if (file is null || file.Length == 0) return BadRequest("No file");

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsDir);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        var record = await subs.AddSubmissionAsync(id, file.FileName, $"/uploads/{fileName}", UserId, UserName);
        return Ok(record);
    }

    [HttpPost("{id:int}/advance")]
    [Authorize(Roles = "SalesArea,AdminRegional")]
    public async Task<ActionResult<AdvanceResult>> Advance(int id)
        => Ok(await workflow.AdvanceStatusAsync(id, UserName));

    [HttpPatch("{id:int}/location")]
    [Authorize(Roles = "SalesArea,AdminRegional")]
    public async Task<IActionResult> UpdateLocation(int id, [FromBody] UpdateLocationRequest req)
        => await subs.UpdateLocationAsync(id, req.Latitude, req.Longitude, UserName)
            ? Ok()
            : NotFound();

    [HttpPost("{id:int}/signoff")]
    [Authorize(Roles = "SalesArea")]
    public async Task<IActionResult> SignOff(int id)
    {
        await workflow.SignOffAsync(id, UserName);
        return Ok();
    }

    [HttpPost("{id:int}/assign-reviewers")]
    [Authorize(Roles = "AdminRegional")]
    public async Task<IActionResult> AssignReviewers(int id, [FromBody] AssignReviewersRequest req)
    {
        await workflow.AssignReviewersAsync(id, req.ReviewerIds);
        return Ok();
    }

    // Every approver role has an inbox (Docs/14), not just the two that sit in ReviewSteps.
    [HttpGet("pending-review")]
    [Authorize(Roles = "AreaHead,AdminRegional,Reviewer,DivisionHead")]
    public async Task<List<SubscriptionDto>> PendingReview() => await workflow.GetPendingReviewAsync(UserId);

    [HttpPost("{id:int}/review")]
    [Authorize(Roles = "Reviewer,DivisionHead")]
    public async Task<IActionResult> SubmitReview(int id, [FromBody] SubmitReviewRequest req)
    {
        var ok = await workflow.SubmitReviewAsync(id, UserId, req, UserName);
        return ok ? Ok() : BadRequest("Cannot review");
    }

    [HttpGet("regions")]
    public async Task<List<RegionDto>> Regions() => await subs.GetRegionsAsync();

    [HttpGet("areas")]
    public async Task<List<AreaDto>> Areas() => await subs.GetAreasAsync();

    [HttpGet("activity/recent")]
    public async Task<List<ActivityLogDto>> RecentActivity([FromQuery] int count = 20) => await subs.GetRecentActivityAsync(count);

    [HttpGet("{id:int}/activity")]
    public async Task<List<ActivityLogDto>> SubscriptionActivity(int id) => await subs.GetSubscriptionActivityAsync(id);

    [HttpGet("users")]
    [Authorize(Roles = "AdminRegional")]
    public async Task<List<UserInfo>> Users() => await subs.GetUsersAsync();
}
