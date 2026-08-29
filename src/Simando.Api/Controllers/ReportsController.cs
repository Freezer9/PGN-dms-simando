using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [ProducesResponseType<FunnelReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFunnel(
        [FromQuery] Guid? areaId = null,
        [FromQuery] Guid? regionId = null,
        CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();

        var data = await reportsService.GetFunnelAsync(currentUser.Permissions, areaId, regionId, ct);
        return Ok(data);
    }

    [HttpGet("gas-demand")]
    [ProducesResponseType<GasDemandReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetGasDemand(CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();

        var data = await reportsService.GetGasDemandAsync(currentUser.Permissions, ct);
        return Ok(data);
    }

    [HttpGet("survey-productivity")]
    [ProducesResponseType<SurveyProductivityReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSurveyProductivity(
        [FromQuery] int? year = null,
        CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();

        var data = await reportsService.GetSurveyProductivityAsync(currentUser.Permissions, year, ct);
        return Ok(data);
    }

    [HttpGet("nol-outcomes")]
    [ProducesResponseType<NolOutcomesReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNolOutcomes(CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();

        var data = await reportsService.GetNolOutcomesAsync(currentUser.Permissions, ct);
        return Ok(data);
    }

    [HttpGet("ageing")]
    [ProducesResponseType<IReadOnlyList<AgeingRow>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAgeing(CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();

        var data = await reportsService.GetAgeingAsync(currentUser.Permissions, ct);
        return Ok(data);
    }
}
