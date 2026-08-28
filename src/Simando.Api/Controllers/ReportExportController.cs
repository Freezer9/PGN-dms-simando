using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simando.Application.Directory;
using Simando.Application.Reports;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Api.Controllers;

[ApiController]
[Route("reports/export")]
[Authorize]
public sealed class ReportExportController(
    IDbContextFactory<SimandoDbContext> dbContextFactory,
    IReportsService reportsService,
    IExcelExportService excelExportService) : ControllerBase
{
    private const string XlsxMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [HttpGet("funnel")]
    public async Task<IActionResult> ExportFunnel(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var permissions = await ResolvePermissionsAsync(db, ct);
        if (permissions is null) return Unauthorized();

        var data = await reportsService.GetFunnelAsync(permissions, null, null, ct);
        var bytes = excelExportService.ExportFunnelReport(data);

        return File(bytes, XlsxMimeType, $"Laporan_Corong_Penjualan_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("gas-demand")]
    public async Task<IActionResult> ExportGasDemand(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var permissions = await ResolvePermissionsAsync(db, ct);
        if (permissions is null) return Unauthorized();

        var data = await reportsService.GetGasDemandAsync(permissions, ct);
        var bytes = excelExportService.ExportGasDemandReport(data);

        return File(bytes, XlsxMimeType, $"Laporan_Potensi_Kebutuhan_Gas_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("survey-productivity")]
    public async Task<IActionResult> ExportSurveyProductivity(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var permissions = await ResolvePermissionsAsync(db, ct);
        if (permissions is null) return Unauthorized();

        var data = await reportsService.GetSurveyProductivityAsync(permissions, null, ct);
        var bytes = excelExportService.ExportSurveyProductivityReport(data);

        return File(bytes, XlsxMimeType, $"Laporan_Produktivitas_Survei_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("nol-outcomes")]
    public async Task<IActionResult> ExportNolOutcomes(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var permissions = await ResolvePermissionsAsync(db, ct);
        if (permissions is null) return Unauthorized();

        var data = await reportsService.GetNolOutcomesAsync(permissions, ct);
        var bytes = excelExportService.ExportNolOutcomesReport(data);

        return File(bytes, XlsxMimeType, $"Laporan_Hasil_NOL_RL_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("directory")]
    public async Task<IActionResult> ExportDirectory([FromQuery] bool includePii = false, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var permissions = await ResolvePermissionsAsync(db, ct);
        if (permissions is null) return Unauthorized();

        // PDP Law (UU 27/2022) compliance check: Only authorized roles with Capability.ExportContactDataPii
        // can request unmasked PII contact export.
        var allowPii = includePii && permissions.HasCapability(Capability.ExportContactDataPii);

        var companies = await db.Companies.AsNoTracking().Take(100).ToListAsync(ct);
        var areaLookup = await db.Areas.AsNoTracking().ToDictionaryAsync(a => a.Id, a => a.Name, ct);
        var industryLookup = await db.IndustryTypes.AsNoTracking().ToDictionaryAsync(i => i.Id, i => i.Name, ct);

        var rows = companies.Select(c => new CompanyDirectoryRow(
            c.Id,
            c.Nomor,
            c.NamaPerusahaan,
            c.Alamat,
            industryLookup.GetValueOrDefault(c.IndustryTypeId, "Industri"),
            areaLookup.GetValueOrDefault(c.AreaId, "Area"),
            c.CurrentStage,
            CompanyLabels.StageLabel(c.CurrentStage),
            "Kontak Utama",
            c.Telp,
            c.Email
        )).ToList();

        var bytes = excelExportService.ExportCompanyDirectory(rows, allowPii);
        var fileName = allowPii
            ? $"Direktori_Perusahaan_PII_{DateTime.UtcNow:yyyyMMdd}.xlsx"
            : $"Direktori_Perusahaan_{DateTime.UtcNow:yyyyMMdd}.xlsx";

        return File(bytes, XlsxMimeType, fileName);
    }

    private async Task<EffectivePermissions?> ResolvePermissionsAsync(SimandoDbContext db, CancellationToken ct)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idClaim is null || !Guid.TryParse(idClaim, out var userId))
            return null;

        var assignments = await db.RoleAssignments
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.Active)
            .ToListAsync(ct);

        return PermissionEvaluator.Resolve(assignments);
    }
}
