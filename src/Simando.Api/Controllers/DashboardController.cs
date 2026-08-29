using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simando.Api.Security;
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
    ICurrentUser currentUser,
    IDbContextFactory<SimandoDbContext> dbContextFactory) : ControllerBase
{
    [HttpGet("stats")]
    [ProducesResponseType<DashboardStatsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboardStats(CancellationToken ct)
    {
        // Determine primary role priority
        if (currentUser.Roles.Contains(Role.SystemAdmin) || currentUser.Permissions.HasCapability(Capability.ManageMasterData))
        {
            var sysData = await dashboardService.GetSystemAdminDashboardAsync(ct);
            return Ok(new DashboardStatsResponse("SystemAdmin", SystemAdmin: sysData));
        }

        if (currentUser.Roles.Contains(Role.RegionalAdmin) || currentUser.Permissions.Scope == AccessScope.Region)
        {
            var regionId = currentUser.Permissions.RegionId;
            if (!regionId.HasValue)
            {
                await using var db = await dbContextFactory.CreateDbContextAsync(ct);
                var firstRegion = await db.Regions.AsNoTracking().FirstOrDefaultAsync(ct);
                regionId = firstRegion?.Id ?? Guid.Empty;
            }

            var regData = await dashboardService.GetRegionalAdminDashboardAsync(regionId.Value, currentUser.Permissions, ct);
            return Ok(new DashboardStatsResponse("RegionalAdmin", RegionalAdmin: regData));
        }

        if (currentUser.Roles.Contains(Role.SalesArea))
        {
            var areaId = currentUser.Permissions.AreaId;
            if (!areaId.HasValue)
            {
                await using var db = await dbContextFactory.CreateDbContextAsync(ct);
                var firstArea = await db.Areas.AsNoTracking().FirstOrDefaultAsync(ct);
                areaId = firstArea?.Id ?? Guid.Empty;
            }

            var salesData = await dashboardService.GetSalesAreaDashboardAsync(areaId.Value, ct);
            return Ok(new DashboardStatsResponse("SalesArea", SalesArea: salesData));
        }

        // Approver / Reviewer / Area Head / Division Head
        var appData = await dashboardService.GetApproverDashboardAsync(currentUser.UserId, currentUser.Permissions, currentUser.Roles, ct);
        var roleName = currentUser.Roles.FirstOrDefault().ToString();
        return Ok(new DashboardStatsResponse(roleName, Approver: appData));
    }

    [HttpGet("sales")]
    [ProducesResponseType<SalesAreaDashboardDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSalesDashboard([FromQuery] Guid? areaId = null, CancellationToken ct = default)
    {
        var targetAreaId = areaId ?? currentUser.Permissions.AreaId;
        if (!targetAreaId.HasValue)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(ct);
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
        var data = await dashboardService.GetApproverDashboardAsync(currentUser.UserId, currentUser.Permissions, currentUser.Roles, ct);
        return Ok(data);
    }

    [HttpGet("regional-admin")]
    [ProducesResponseType<RegionalAdminDashboardDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRegionalAdminDashboard([FromQuery] Guid? regionId = null, CancellationToken ct = default)
    {
        var targetRegionId = regionId ?? currentUser.Permissions.RegionId;
        if (!targetRegionId.HasValue)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(ct);
            var firstRegion = await db.Regions.AsNoTracking().FirstOrDefaultAsync(ct);
            targetRegionId = firstRegion?.Id;
        }

        if (!targetRegionId.HasValue)
        {
            return BadRequest("Region ID tidak ditemukan.");
        }

        var data = await dashboardService.GetRegionalAdminDashboardAsync(targetRegionId.Value, currentUser.Permissions, ct);
        return Ok(data);
    }

    [HttpGet("system-admin")]
    [RequireCapability(Capability.ManageMasterData)]
    [ProducesResponseType<SystemAdminDashboardDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSystemAdminDashboard(CancellationToken ct)
    {
        var data = await dashboardService.GetSystemAdminDashboardAsync(ct);
        return Ok(data);
    }
}
