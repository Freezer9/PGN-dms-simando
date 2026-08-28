using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simando.Application.Common;
using Simando.Application.Security;
using Simando.Application.Workflow;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Persistence;

namespace Simando.Api.Controllers;

public sealed record StuckStepItemDto(
    Guid StepId,
    Guid InstanceId,
    Guid CompanyId,
    string CompanyNomor,
    string CompanyName,
    Guid RegionId,
    string RegionName,
    Guid AreaId,
    string AreaName,
    WorkflowStepKind StepKind,
    Guid? AssignedUserId,
    string AssignedUserName,
    DateTimeOffset StartedAt,
    int ElapsedDays
);

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
    IDbContextFactory<SimandoDbContext> dbContextFactory) : ControllerBase
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
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actor = await ResolveActorContextAsync(db, ct);
        if (actor is null) return Unauthorized();

        var (_, permissions, _) = actor.Value;
        if (!permissions.HasCapability(Capability.ViewBreakGlassActivity) && permissions.Scope != AccessScope.All)
        {
            return Forbid();
        }

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var logs = await breakGlassService.GetPagedAuditLogsAsync(permissions, page, pageSize, ct);
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

        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actor = await ResolveActorContextAsync(db, ct);
        if (actor is null) return Unauthorized();

        var (userId, permissions, _) = actor.Value;
        if (!permissions.HasCapability(Capability.BreakGlassRecordRead))
        {
            return Forbid();
        }

        var access = await breakGlassService.RequestAccessAsync(
            request.CompanyId,
            request.Reason.Trim(),
            userId,
            permissions,
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
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actor = await ResolveActorContextAsync(db, ct);
        if (actor is null) return Unauthorized();

        var (_, permissions, _) = actor.Value;
        if (!permissions.HasCapability(Capability.ManageMasterData) && permissions.Scope != AccessScope.All)
        {
            return Forbid();
        }

        var instances = await db.WorkflowInstances.AsNoTracking()
            .Where(i => i.CompletedAt == null)
            .ToListAsync(ct);

        var instanceIds = instances.Select(i => i.Id).ToHashSet();
        var companyIds = instances.Select(i => i.CompanyId).ToHashSet();

        var steps = await db.WorkflowSteps.AsNoTracking()
            .Where(s => instanceIds.Contains(s.WorkflowInstanceId) && s.ActedAt == null)
            .ToListAsync(ct);

        var companies = await db.Companies.IgnoreQueryFilters().AsNoTracking()
            .Where(c => companyIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, ct);

        var areaIds = companies.Values.Select(c => c.AreaId).ToHashSet();
        var areas = await db.Areas.AsNoTracking()
            .Where(a => areaIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, ct);

        var regionIds = areas.Values.Select(a => a.RegionId).ToHashSet();
        var regions = await db.Regions.AsNoTracking()
            .Where(r => regionIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.Name, ct);

        var assignedUserIds = steps.Where(s => s.AssignedUserId.HasValue).Select(s => s.AssignedUserId!.Value).ToHashSet();
        var users = await db.Users.AsNoTracking()
            .Where(u => assignedUserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, ct);

        var instanceDict = instances.ToDictionary(i => i.Id);
        var now = DateTimeOffset.UtcNow;

        var result = new List<StuckStepItemDto>();
        foreach (var step in steps)
        {
            if (!instanceDict.TryGetValue(step.WorkflowInstanceId, out var inst)) continue;
            if (!companies.TryGetValue(inst.CompanyId, out var comp)) continue;

            var area = areas.GetValueOrDefault(comp.AreaId);
            var regionName = area is not null ? regions.GetValueOrDefault(area.RegionId, "-") : "-";
            var areaName = area?.Name ?? "-";
            var regionId = area?.RegionId ?? Guid.Empty;

            var startedAt = inst.StartedAt;
            var elapsedDays = Math.Max(0, (int)(now - startedAt).TotalDays);

            var assignedName = step.AssignedUserId.HasValue
                ? users.GetValueOrDefault(step.AssignedUserId.Value, "Ditugaskan")
                : $"Peran: {step.Kind}";

            result.Add(new StuckStepItemDto(
                step.Id,
                inst.Id,
                comp.Id,
                comp.Nomor,
                comp.NamaPerusahaan,
                regionId,
                regionName,
                comp.AreaId,
                areaName,
                step.Kind,
                step.AssignedUserId,
                assignedName,
                startedAt,
                elapsedDays
            ));
        }

        // Sort descending by elapsed days (oldest first)
        return Ok(result.OrderByDescending(s => s.ElapsedDays).ToList());
    }

    [HttpPost("stuck-steps/reassign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReassignStuckStep([FromBody] AdminReassignStepRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actor = await ResolveActorContextAsync(db, ct);
        if (actor is null) return Unauthorized();

        var (userId, permissions, _) = actor.Value;
        if (!permissions.HasCapability(Capability.ReassignWorkflowStep) && permissions.Scope != AccessScope.All)
        {
            return Forbid();
        }

        var result = await workflowService.ReassignStepAsync(
            request.StepId,
            request.TargetUserId,
            userId,
            permissions,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
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
