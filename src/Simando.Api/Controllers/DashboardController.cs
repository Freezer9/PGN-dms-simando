using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simando.Application.Dashboard;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Api.Controllers;

public sealed record DashboardStatsResponse(
    string Role,
    SalesAreaDashboardDto? SalesArea = null,
    ApproverDashboardDto? Approver = null,
    RegionalAdminDashboardDto? RegionalAdmin = null,
    SystemAdminDashboardDto? SystemAdmin = null
);

[ApiController]
[Route("api/dashboard")]
[Authorize]
public sealed class DashboardController(
    IDashboardService dashboardService,
    IDbContextFactory<SimandoDbContext> dbContextFactory) : ControllerBase
{
    [HttpGet("stats")]
    [ProducesResponseType<DashboardStatsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboardStats(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actor = await ResolveActorContextAsync(db, ct);
        if (actor is null) return Unauthorized();

        var (userId, permissions, roles) = actor.Value;

        // Determine primary role priority
        if (roles.Contains(Role.SystemAdmin) || permissions.HasCapability(Capability.ManageMasterData))
        {
            var sysData = await dashboardService.GetSystemAdminDashboardAsync(ct);
            return Ok(new DashboardStatsResponse("SystemAdmin", SystemAdmin: sysData));
        }

        if (roles.Contains(Role.RegionalAdmin) || permissions.Scope == AccessScope.Region)
        {
            var regionId = permissions.RegionId;
            if (!regionId.HasValue)
            {
                var firstRegion = await db.Regions.AsNoTracking().FirstOrDefaultAsync(ct);
                regionId = firstRegion?.Id ?? Guid.Empty;
            }

            var regData = await dashboardService.GetRegionalAdminDashboardAsync(regionId.Value, permissions, ct);
            return Ok(new DashboardStatsResponse("RegionalAdmin", RegionalAdmin: regData));
        }

        if (roles.Contains(Role.SalesArea))
        {
            var areaId = permissions.AreaId;
            if (!areaId.HasValue)
            {
                var firstArea = await db.Areas.AsNoTracking().FirstOrDefaultAsync(ct);
                areaId = firstArea?.Id ?? Guid.Empty;
            }

            var salesData = await dashboardService.GetSalesAreaDashboardAsync(areaId.Value, ct);
            return Ok(new DashboardStatsResponse("SalesArea", SalesArea: salesData));
        }

        // Approver / Reviewer / Area Head / Division Head
        var appData = await dashboardService.GetApproverDashboardAsync(userId, permissions, roles, ct);
        var roleName = roles.FirstOrDefault().ToString();
        return Ok(new DashboardStatsResponse(roleName, Approver: appData));
    }

    [HttpGet("sales")]
    [ProducesResponseType<SalesAreaDashboardDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSalesDashboard([FromQuery] Guid? areaId = null, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actor = await ResolveActorContextAsync(db, ct);
        if (actor is null) return Unauthorized();

        var targetAreaId = areaId ?? actor.Value.Permissions.AreaId;
        if (!targetAreaId.HasValue)
        {
            var firstArea = await db.Areas.AsNoTracking().FirstOrDefaultAsync(ct);
            targetAreaId = firstArea?.Id;
        }

        if (!targetAreaId.HasValue)
        {
            return BadRequest("Area ID tidak ditemukan.");
        }

        var data = await dashboardService.GetSalesAreaDashboardAsync(targetAreaId.Value, ct);
        return Ok(data);
    }

    [HttpGet("approver")]
    [ProducesResponseType<ApproverDashboardDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetApproverDashboard(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actor = await ResolveActorContextAsync(db, ct);
        if (actor is null) return Unauthorized();

        var (userId, permissions, roles) = actor.Value;
        var data = await dashboardService.GetApproverDashboardAsync(userId, permissions, roles, ct);
        return Ok(data);
    }

    [HttpGet("regional-admin")]
    [ProducesResponseType<RegionalAdminDashboardDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRegionalAdminDashboard([FromQuery] Guid? regionId = null, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actor = await ResolveActorContextAsync(db, ct);
        if (actor is null) return Unauthorized();

        var targetRegionId = regionId ?? actor.Value.Permissions.RegionId;
        if (!targetRegionId.HasValue)
        {
            var firstRegion = await db.Regions.AsNoTracking().FirstOrDefaultAsync(ct);
            targetRegionId = firstRegion?.Id;
        }

        if (!targetRegionId.HasValue)
        {
            return BadRequest("Region ID tidak ditemukan.");
        }

        var data = await dashboardService.GetRegionalAdminDashboardAsync(targetRegionId.Value, actor.Value.Permissions, ct);
        return Ok(data);
    }

    [HttpGet("system-admin")]
    [ProducesResponseType<SystemAdminDashboardDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSystemAdminDashboard(CancellationToken ct)
    {
        var data = await dashboardService.GetSystemAdminDashboardAsync(ct);
        return Ok(data);
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
