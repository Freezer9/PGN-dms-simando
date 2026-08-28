using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simando.Application.Common;
using Simando.Application.Tasks;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Api.Controllers;

public sealed record TasksSummaryDto(int MyTasksCount, int RegionTasksCount, int BlockedTasksCount);

[ApiController]
[Route("api/tasks")]
[Authorize]
public sealed class TasksController(
    ITasksService tasksService,
    IDbContextFactory<SimandoDbContext> dbContextFactory) : ControllerBase
{
    [HttpGet("inbox")]
    [ProducesResponseType<IReadOnlyList<TaskListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetInbox(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actorContext = await ResolveActorContextAsync(db, ct);
        if (actorContext is null)
        {
            return Unauthorized();
        }

        var tasks = await tasksService.GetMyTasksAsync(
            actorContext.Value.UserId,
            actorContext.Value.Permissions,
            actorContext.Value.Roles,
            ct);

        return Ok(tasks);
    }

    [HttpGet("region")]
    [ProducesResponseType<IReadOnlyList<TaskListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRegion(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actorContext = await ResolveActorContextAsync(db, ct);
        if (actorContext is null)
        {
            return Unauthorized();
        }

        var tasks = await tasksService.GetRegionTasksAsync(
            actorContext.Value.Permissions,
            ct);

        return Ok(tasks);
    }

    [HttpGet("blocked")]
    [ProducesResponseType<IReadOnlyList<TaskListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBlocked(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actorContext = await ResolveActorContextAsync(db, ct);
        if (actorContext is null)
        {
            return Unauthorized();
        }

        var tasks = await tasksService.GetBlockedTasksAsync(
            actorContext.Value.Permissions,
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
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actorContext = await ResolveActorContextAsync(db, ct);
        if (actorContext is null)
        {
            return Unauthorized();
        }

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var history = await tasksService.GetPagedHistoryAsync(
            actorContext.Value.UserId,
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
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actorContext = await ResolveActorContextAsync(db, ct);
        if (actorContext is null)
        {
            return Unauthorized();
        }

        var myTasks = await tasksService.GetMyTasksAsync(
            actorContext.Value.UserId,
            actorContext.Value.Permissions,
            actorContext.Value.Roles,
            ct);

        var regionTasks = await tasksService.GetRegionTasksAsync(
            actorContext.Value.Permissions,
            ct);

        var blockedTasks = await tasksService.GetBlockedTasksAsync(
            actorContext.Value.Permissions,
            ct);

        return Ok(new TasksSummaryDto(myTasks.Count, regionTasks.Count, blockedTasks.Count));
    }

    private async Task<(Guid UserId, EffectivePermissions Permissions, IReadOnlySet<Role> Roles)?> ResolveActorContextAsync(
        SimandoDbContext db,
        CancellationToken ct)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idClaim is null || !Guid.TryParse(idClaim, out var userId))
        {
            return null;
        }

        var assignments = await db.RoleAssignments
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.Active)
            .ToListAsync(ct);

        var permissions = PermissionEvaluator.Resolve(assignments);
        var roles = assignments.Select(a => a.Role).ToHashSet();

        return (userId, permissions, roles);
    }
}
