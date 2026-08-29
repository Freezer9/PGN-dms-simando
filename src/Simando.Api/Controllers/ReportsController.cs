using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Api.Security;
using Simando.Application.Reports;
using Simando.Domain.Security;

namespace Simando.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public sealed class ReportsController(
    IReportsService reportsService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("funnel")]
    [RequireCapability(Capability.ViewDashboardFunnel)]
    [ProducesResponseType<FunnelReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFunnel(
        [FromQuery] TerritoryReportQuery query,
        CancellationToken ct = default)
    {
        var data = await reportsService.GetFunnelAsync(currentUser.Permissions, query.AreaId, query.RegionId, ct);
        return Ok(data);
    }

    [HttpGet("gas-demand")]
    [RequireCapability(Capability.ViewDashboardFunnel)]
    [ProducesResponseType<GasDemandReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetGasDemand(CancellationToken ct)
    {
        var data = await reportsService.GetGasDemandAsync(currentUser.Permissions, ct);
        return Ok(data);
    }

    [HttpGet("survey-productivity")]
    [RequireCapability(Capability.ViewDashboardFunnel)]
    [ProducesResponseType<SurveyProductivityReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSurveyProductivity(
        [FromQuery] SurveyProductivityReportQuery query,
        CancellationToken ct = default)
    {
        var data = await reportsService.GetSurveyProductivityAsync(currentUser.Permissions, query.Year, ct);
        return Ok(data);
    }

    [HttpGet("nol-outcomes")]
    [RequireCapability(Capability.ViewDashboardFunnel)]
    [ProducesResponseType<NolOutcomesReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNolOutcomes(CancellationToken ct)
    {
        var data = await reportsService.GetNolOutcomesAsync(currentUser.Permissions, ct);
        return Ok(data);
    }

    [HttpGet("ageing")]
    [RequireCapability(Capability.ViewAgeingReport)]
    [ProducesResponseType<IReadOnlyList<AgeingRow>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAgeing(CancellationToken ct)
    {
        var data = await reportsService.GetAgeingAsync(currentUser.Permissions, ct);
        return Ok(data);
    }
}
