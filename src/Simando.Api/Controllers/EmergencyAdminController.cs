using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [ProducesResponseType<PagedResult<BreakGlassAccessDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetBreakGlassLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();

        if (!currentUser.Permissions.HasCapability(Capability.ViewBreakGlassActivity) && currentUser.Scope != AccessScope.All)
        {
            return Forbid();
        }

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var logs = await breakGlassService.GetPagedAuditLogsAsync(currentUser.Permissions, page, pageSize, ct);
        return Ok(logs);
    }

    [HttpPost("break-glass/request")]
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

        if (!currentUser.IsAuthenticated) return Unauthorized();

        if (!currentUser.Permissions.HasCapability(Capability.BreakGlassRecordRead))
        {
            return Forbid();
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
    [ProducesResponseType<IReadOnlyList<StuckStepItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStuckSteps(CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();

        if (!currentUser.Permissions.HasCapability(Capability.ManageMasterData) && currentUser.Scope != AccessScope.All)
        {
            return Forbid();
        }

        var result = await workflowService.GetStuckStepsAsync(currentUser.Permissions, ct);
        return Ok(result);
    }

    [HttpPost("stuck-steps/reassign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReassignStuckStep([FromBody] AdminReassignStepRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();

        if (!currentUser.Permissions.HasCapability(Capability.ReassignWorkflowStep) && currentUser.Scope != AccessScope.All)
        {
            return Forbid();
        }

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
