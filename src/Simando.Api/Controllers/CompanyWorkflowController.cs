using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Api.Security;
using Simando.Application.Directory;
using Simando.Application.Workflow;
using Simando.Domain.Security;

namespace Simando.Api.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize]
public sealed class CompanyWorkflowController(
    IWorkflowService workflowService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpPost("{id:guid}/workflow/start")]
    [RequireCapability(Capability.SubmitForApproval)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartWorkflow(Guid id, CancellationToken ct)
    {
        var result = await workflowService.StartAsync(
            id,
            currentUser.UserId,
            currentUser.Permissions,
            currentUser.Roles,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Workflow Start Failed",
                Detail = result.Error
            });
        }

        return Ok();
    }

    [HttpPost("{id:guid}/workflow/choose-reviewers")]
    [RequireCapability(Capability.ChooseReviewers)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ChooseReviewers(Guid id, [FromBody] ChooseReviewersRequest request, CancellationToken ct)
    {
        var result = await workflowService.ChooseReviewersAsync(
            id,
            request.ReviewerUserIds,
            currentUser.UserId,
            currentUser.Permissions,
            currentUser.Roles,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Assign Reviewers Failed",
                Detail = result.Error
            });
        }

        return Ok();
    }

    [HttpPost("{id:guid}/workflow/rework")]
    [RequireCapability(Capability.ActOnApprovalStep, Capability.ReassignWorkflowStep)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Rework(Guid id, [FromBody] ReworkRequest request, CancellationToken ct)
    {
        var result = await workflowService.ReworkAsync(
            id,
            request.Comment,
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Rework Failed",
                Detail = result.Error
            });
        }

        return Ok();
    }

    [HttpPost("{id:guid}/workflow/discontinue")]
    [RequireCapability(Capability.EditStages1To3, Capability.ReassignWorkflowStep, Capability.SoftDeleteCompany)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Discontinue(Guid id, [FromBody] DiscontinueRequest request, CancellationToken ct)
    {
        var result = await workflowService.DiscontinueAsync(
            id,
            request.Comment,
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Discontinue Failed",
                Detail = result.Error
            });
        }

        return Ok();
    }
}
