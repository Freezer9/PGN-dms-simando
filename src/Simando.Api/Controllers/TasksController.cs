using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Application.Common;
using Simando.Application.Tasks;
using Simando.Domain.Security;

namespace Simando.Api.Controllers;

public sealed record TasksSummaryDto(int MyTasksCount, int RegionTasksCount, int BlockedTasksCount);

[ApiController]
[Route("api/tasks")]
[Authorize]
public sealed class TasksController(
    ITasksService tasksService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("inbox")]
    [ProducesResponseType<IReadOnlyList<TaskListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetInbox(CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

        var tasks = await tasksService.GetMyTasksAsync(
            currentUser.UserId,
            currentUser.Permissions,
            currentUser.Roles,
            ct);

        return Ok(tasks);
    }

    [HttpGet("region")]
    [ProducesResponseType<IReadOnlyList<TaskListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRegion(CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

        var tasks = await tasksService.GetRegionTasksAsync(
            currentUser.Permissions,
            ct);

        return Ok(tasks);
    }

    [HttpGet("blocked")]
    [ProducesResponseType<IReadOnlyList<TaskListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBlocked(CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

        var tasks = await tasksService.GetBlockedTasksAsync(
            currentUser.Permissions,
            ct);

        return Ok(tasks);
    }

    [HttpGet("history")]
    [ProducesResponseType<PagedResult<TaskHistoryItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var history = await tasksService.GetPagedHistoryAsync(
            currentUser.UserId,
            page,
            pageSize,
            ct);

        return Ok(history);
    }

    [HttpGet("summary")]
    [ProducesResponseType<TasksSummaryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

        var myTasks = await tasksService.GetMyTasksAsync(
            currentUser.UserId,
            currentUser.Permissions,
            currentUser.Roles,
            ct);

        var regionTasks = await tasksService.GetRegionTasksAsync(
            currentUser.Permissions,
            ct);

        var blockedTasks = await tasksService.GetBlockedTasksAsync(
            currentUser.Permissions,
            ct);

        return Ok(new TasksSummaryDto(myTasks.Count, regionTasks.Count, blockedTasks.Count));
    }
}
