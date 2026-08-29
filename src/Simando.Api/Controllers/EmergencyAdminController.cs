using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Api.Security;
using Simando.Application.Common;
using Simando.Application.Security;
using Simando.Application.Workflow;
using Simando.Domain.Security;

namespace Simando.Api.Controllers;

public sealed record AdminReassignStepRequest(
    Guid StepId,
    Guid TargetUserId
);

public sealed record BreakGlassRequest(
    Guid CompanyId,
    string Reason
);

[ApiController]
[Route("api/admin")]
[Authorize]
public sealed class EmergencyAdminController(
    IBreakGlassService breakGlassService,
    IWorkflowService workflowService,
    ICurrentUser currentUser) : ControllerBase
{
    // ==========================================
    // 1. Break Glass Emergency Access
    // ==========================================
    [HttpGet("break-glass/logs")]
    [RequireCapability(Capability.ViewBreakGlassActivity)]
    [ProducesResponseType<PagedResult<BreakGlassAccessDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetBreakGlassLogs(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 10 : (query.PageSize > 100 ? 100 : query.PageSize);

        var logs = await breakGlassService.GetPagedAuditLogsAsync(currentUser.Permissions, page, pageSize, ct);
        return Ok(logs);
    }

    [HttpPost("break-glass/request")]
    [RequireCapability(Capability.BreakGlassRecordRead)]
    [ProducesResponseType<BreakGlassAccessDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RequestBreakGlass([FromBody] BreakGlassRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest(new { error = "Alasan akses darurat wajib diisi." });
        }

        var access = await breakGlassService.RequestAccessAsync(
            request.CompanyId,
            request.Reason.Trim(),
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (access is null)
        {
            return BadRequest(new { error = "Permintaan akses darurat ditolak atau perusahaan tidak ditemukan." });
        }

        return Ok(access);
    }

    // ==========================================
    // 2. Cross-Region Stuck Steps (System Admin)
    // ==========================================
    [HttpGet("stuck-steps")]
    [RequireCapability(Capability.ManageMasterData)]
    [ProducesResponseType<IReadOnlyList<StuckStepItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStuckSteps(CancellationToken ct)
    {
        var result = await workflowService.GetStuckStepsAsync(currentUser.Permissions, ct);
        return Ok(result);
    }

    [HttpPost("stuck-steps/reassign")]
    [RequireCapability(Capability.ReassignWorkflowStep)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReassignStuckStep([FromBody] AdminReassignStepRequest request, CancellationToken ct)
    {
        var result = await workflowService.ReassignStepAsync(
            request.StepId,
            request.TargetUserId,
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }
}
