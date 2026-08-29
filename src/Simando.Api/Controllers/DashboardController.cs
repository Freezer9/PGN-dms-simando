using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Api.Security;
using Simando.Application.Dashboard;
using Simando.Domain.Security;

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
    ICurrentUser currentUser) : ControllerBase
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
            var regData = await dashboardService.GetRegionalAdminDashboardAsync(currentUser.Permissions.RegionId, currentUser.Permissions, ct);
            return Ok(new DashboardStatsResponse("RegionalAdmin", RegionalAdmin: regData));
        }

        if (currentUser.Roles.Contains(Role.SalesArea))
        {
            var salesData = await dashboardService.GetSalesAreaDashboardAsync(currentUser.Permissions.AreaId, ct);
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
        var data = await dashboardService.GetSalesAreaDashboardAsync(targetAreaId, ct);
        if (data is null)
        {
            return BadRequest("Area ID tidak ditemukan.");
        }

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
        var data = await dashboardService.GetRegionalAdminDashboardAsync(targetRegionId, currentUser.Permissions, ct);
        if (data is null)
        {
            return BadRequest("Region ID tidak ditemukan.");
        }

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
