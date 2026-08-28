using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simando.Application.Reports;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public sealed class ReportsController(
    IReportsService reportsService,
    IDbContextFactory<SimandoDbContext> dbContextFactory) : ControllerBase
{
    [HttpGet("funnel")]
    [ProducesResponseType<FunnelReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFunnel(
        [FromQuery] Guid? areaId = null,
        [FromQuery] Guid? regionId = null,
        CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var permissions = await ResolvePermissionsAsync(db, ct);
        if (permissions is null) return Unauthorized();

        var data = await reportsService.GetFunnelAsync(permissions, areaId, regionId, ct);
        return Ok(data);
    }

    [HttpGet("gas-demand")]
    [ProducesResponseType<GasDemandReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetGasDemand(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var permissions = await ResolvePermissionsAsync(db, ct);
        if (permissions is null) return Unauthorized();

        var data = await reportsService.GetGasDemandAsync(permissions, ct);
        return Ok(data);
    }

    [HttpGet("survey-productivity")]
    [ProducesResponseType<SurveyProductivityReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSurveyProductivity(
        [FromQuery] int? year = null,
        CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var permissions = await ResolvePermissionsAsync(db, ct);
        if (permissions is null) return Unauthorized();

        var data = await reportsService.GetSurveyProductivityAsync(permissions, year, ct);
        return Ok(data);
    }

    [HttpGet("nol-outcomes")]
    [ProducesResponseType<NolOutcomesReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNolOutcomes(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var permissions = await ResolvePermissionsAsync(db, ct);
        if (permissions is null) return Unauthorized();

        var data = await reportsService.GetNolOutcomesAsync(permissions, ct);
        return Ok(data);
    }

    [HttpGet("ageing")]
    [ProducesResponseType<IReadOnlyList<AgeingRow>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAgeing(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var permissions = await ResolvePermissionsAsync(db, ct);
        if (permissions is null) return Unauthorized();

        var data = await reportsService.GetAgeingAsync(permissions, ct);
        return Ok(data);
    }

    private async Task<EffectivePermissions?> ResolvePermissionsAsync(SimandoDbContext db, CancellationToken ct)
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

        return PermissionEvaluator.Resolve(assignments);
    }
}
