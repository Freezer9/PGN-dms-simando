using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Api.Security;
using Simando.Application.Reports;
using Simando.Domain.Security;

namespace Simando.Api.Controllers;

[ApiController]
[Route("api/reports/export")]
[Route("reports/export")]
[Authorize]
[RequireCapability(Capability.ExportExcel)]
public sealed class ReportExportController(
    IReportsService reportsService,
    IExcelExportService excelExportService,
    ICurrentUser currentUser) : ControllerBase
{
    private const string XlsxMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [HttpGet("funnel")]
    [Produces(XlsxMimeType)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportFunnel(
        [FromQuery] TerritoryReportQuery query,
        CancellationToken ct = default)
    {
        var data = await reportsService.GetFunnelAsync(currentUser.Permissions, query.AreaId, query.RegionId, ct);
        var bytes = excelExportService.ExportFunnelReport(data);

        return File(bytes, XlsxMimeType, $"Laporan_Corong_Penjualan_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("gas-demand")]
    [Produces(XlsxMimeType)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportGasDemand(CancellationToken ct)
    {
        var data = await reportsService.GetGasDemandAsync(currentUser.Permissions, ct);
        var bytes = excelExportService.ExportGasDemandReport(data);

        return File(bytes, XlsxMimeType, $"Laporan_Potensi_Kebutuhan_Gas_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("survey-productivity")]
    [Produces(XlsxMimeType)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportSurveyProductivity([FromQuery] SurveyProductivityReportQuery query, CancellationToken ct = default)
    {
        var data = await reportsService.GetSurveyProductivityAsync(currentUser.Permissions, query.Year, ct);
        var bytes = excelExportService.ExportSurveyProductivityReport(data);

        return File(bytes, XlsxMimeType, $"Laporan_Produktivitas_Survei_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("nol-outcomes")]
    [Produces(XlsxMimeType)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportNolOutcomes(CancellationToken ct)
    {
        var data = await reportsService.GetNolOutcomesAsync(currentUser.Permissions, ct);
        var bytes = excelExportService.ExportNolOutcomesReport(data);

        return File(bytes, XlsxMimeType, $"Laporan_Hasil_NOL_RL_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("ageing")]
    [Produces(XlsxMimeType)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportAgeing(CancellationToken ct)
    {
        var data = await reportsService.GetAgeingAsync(currentUser.Permissions, ct);
        var bytes = excelExportService.ExportAgeingReport(data);

        return File(bytes, XlsxMimeType, $"Laporan_Penuaan_Workflow_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("directory")]
    [Produces(XlsxMimeType)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportDirectory([FromQuery] DirectoryExportQuery query, CancellationToken ct = default)
    {
        // PDP Law (UU 27/2022) compliance check: Only authorized roles with Capability.ExportContactDataPii
        // can request unmasked PII contact export.
        var allowPii = query.IncludePii && currentUser.Permissions.HasCapability(Capability.ExportContactDataPii);

        var rows = await reportsService.GetCompanyDirectoryRowsAsync(ct);
        var bytes = excelExportService.ExportCompanyDirectory(rows, allowPii);
        var fileName = allowPii
            ? $"Direktori_Perusahaan_PII_{DateTime.UtcNow:yyyyMMdd}.xlsx"
            : $"Direktori_Perusahaan_{DateTime.UtcNow:yyyyMMdd}.xlsx";

        return File(bytes, XlsxMimeType, fileName);
    }
}
