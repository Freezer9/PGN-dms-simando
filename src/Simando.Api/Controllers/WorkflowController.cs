using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simando.Application.Workflow;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Persistence;

namespace Simando.Api.Controllers;

public sealed record ActOnStepRequest(WorkflowAction Action, string? Comment);

[ApiController]
[Route("api/workflow")]
[Authorize]
public sealed class WorkflowController(
    IWorkflowService workflowService,
    IDbContextFactory<SimandoDbContext> dbContextFactory) : ControllerBase
{
    [HttpPost("steps/{stepId:guid}/act")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Act(Guid stepId, [FromBody] ActOnStepRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actorContext = await ResolveActorContextAsync(db, ct);
        if (actorContext is null)
        {
            return Unauthorized();
        }

        var result = await workflowService.ActAsync(
            stepId,
            request.Action,
            request.Comment,
            actorContext.Value.UserId,
            actorContext.Value.Permissions,
            actorContext.Value.Roles,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Workflow Action Rejected",
                Detail = result.Error,
            });
        }

        return Ok();
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
