using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Api.Security;
using Simando.Application.Workflow;
using Simando.Domain.Security;
using Simando.Domain.Workflow;

namespace Simando.Api.Controllers;

public sealed record ActOnStepRequest(WorkflowAction Action, string? Comment);
public sealed record ReassignStepRequest(Guid NewUserId, string? Reason);

[ApiController]
[Route("api/workflow")]
[Authorize]
public sealed class WorkflowController(
    IWorkflowService workflowService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpPost("steps/{stepId:guid}/act")]
    [RequireCapability(Capability.ActOnApprovalStep)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Act(Guid stepId, [FromBody] ActOnStepRequest request, CancellationToken ct)
    {
        var result = await workflowService.ActAsync(
            stepId,
            request.Action,
            request.Comment,
            currentUser.UserId,
            currentUser.Permissions,
            currentUser.Roles,
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

    [HttpPost("steps/{stepId:guid}/reassign")]
    [RequireCapability(Capability.ReassignWorkflowStep)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Reassign(Guid stepId, [FromBody] ReassignStepRequest request, CancellationToken ct)
    {
        var result = await workflowService.ReassignStepAsync(
            stepId,
            request.NewUserId,
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Reassignment Rejected",
                Detail = result.Error,
            });
        }

        return Ok();
    }
}
