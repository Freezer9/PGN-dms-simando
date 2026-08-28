namespace Simando.Application.Reports;

public sealed record CompanyDirectoryRow(
    Guid Id,
    string Nomor,
    string NamaPerusahaan,
    string Alamat,
    string IndustryTypeName,
    string AreaName,
    byte CurrentStage,
    string StageName,
    string? ContactName,
    string? ContactPhone,
    string? ContactEmail
);

public interface IExcelExportService
{
    byte[] ExportFunnelReport(FunnelReportDto report);
    byte[] ExportGasDemandReport(GasDemandReportDto report);
    byte[] ExportSurveyProductivityReport(SurveyProductivityReportDto report);
    byte[] ExportNolOutcomesReport(NolOutcomesReportDto report);
    byte[] ExportAgeingReport(IReadOnlyList<AgeingRow> rows);
    byte[] ExportCompanyDirectory(IReadOnlyList<CompanyDirectoryRow> rows, bool includePiiContactData);
}
