using ClosedXML.Excel;
using Simando.Application.Reports;

namespace Simando.Infrastructure.Reports;

internal sealed class ExcelExportService : IExcelExportService
{
    public byte[] ExportFunnelReport(FunnelReportDto report)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Corong Penjualan");

        // Title Header
        ws.Cell(1, 1).Value = "LAPORAN CORONG PENJUALAN (SALES FUNNEL)";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(2, 1).Value = $"Total Record: {report.TotalRecords} | Konversi Akhir: {report.OverallConversionRatePct:N1}%";
        ws.Cell(2, 1).Style.Font.Italic = true;

        // Table Header
        var headers = new[] { "Tahap", "Nama Tahap", "Jumlah Record", "Tingkat Konversi (%)", "Rata-Rata Waktu (Hari)" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E3A8A");
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Data Rows
        int row = 5;
        foreach (var stage in report.Stages)
        {
            ws.Cell(row, 1).Value = stage.Stage;
            ws.Cell(row, 2).Value = stage.StageName;
            ws.Cell(row, 3).Value = stage.RecordCount;
            ws.Cell(row, 4).Value = stage.ConversionRatePct;
            ws.Cell(row, 5).Value = stage.AvgTurnaroundDays;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] ExportGasDemandReport(GasDemandReportDto report)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Potensi Kebutuhan Gas");

        ws.Cell(1, 1).Value = "LAPORAN POTENSI KEBUTUHAN ENERGI (GAS DEMAND PIPELINE)";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(2, 1).Value = $"Total Kebutuhan Energi: {report.GrandTotalDemandMMBtu:N2} MMBtu";
        ws.Cell(2, 1).Style.Font.Italic = true;

        ws.Cell(4, 1).Value = "KEBUTUHAN GAS PER TAHAP";
        ws.Cell(4, 1).Style.Font.Bold = true;

        var headers = new[] { "Tahap", "Nama Tahap", "Jumlah Record", "Total Kebutuhan (MMBtu)" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(5, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E3A8A");
            cell.Style.Font.FontColor = XLColor.White;
        }

        int row = 6;
        foreach (var s in report.ByStage)
        {
            ws.Cell(row, 1).Value = s.Stage;
            ws.Cell(row, 2).Value = s.StageName;
            ws.Cell(row, 3).Value = s.RecordCount;
            ws.Cell(row, 4).Value = s.TotalDemandMMBtu;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] ExportSurveyProductivityReport(SurveyProductivityReportDto report)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Produktivitas Survei");

        ws.Cell(1, 1).Value = "LAPORAN PRODUKTIVITAS SURVEI (KK0)";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(2, 1).Value = $"Total KK0 Selesai: {report.TotalSurveysCompleted}";
        ws.Cell(2, 1).Style.Font.Italic = true;

        var headers = new[] { "Sales Rep", "Sales Area", "Bulan", "Tahun", "Jumlah KK0 Selesai", "Rata-Rata Hari / Survei" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E3A8A");
            cell.Style.Font.FontColor = XLColor.White;
        }

        int row = 5;
        foreach (var r in report.Rows)
        {
            ws.Cell(row, 1).Value = r.SalesRepName;
            ws.Cell(row, 2).Value = r.AreaName;
            ws.Cell(row, 3).Value = r.Month;
            ws.Cell(row, 4).Value = r.Year;
            ws.Cell(row, 5).Value = r.SurveysCompletedCount;
            ws.Cell(row, 6).Value = r.AvgDaysPerSurvey;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] ExportNolOutcomesReport(NolOutcomesReportDto report)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Hasil NOL & RL");

        ws.Cell(1, 1).Value = "LAPORAN HASIL PENERBITAN NOL / RL";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(2, 1).Value = $"Total Evaluasi: {report.TotalEvaluated} | NOL: {report.NolCount} ({report.NolPercentage:N1}%) | RL: {report.RlCount} ({report.RlPercentage:N1}%)";
        ws.Cell(2, 1).Style.Font.Italic = true;

        var headers = new[] { "Kategori Alasan Penolakan / Syarat", "Jumlah Record", "Persentase (%)" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E3A8A");
            cell.Style.Font.FontColor = XLColor.White;
        }

        int row = 5;
        foreach (var r in report.RejectionReasons)
        {
            ws.Cell(row, 1).Value = r.ReasonCategoryName;
            ws.Cell(row, 2).Value = r.Count;
            ws.Cell(row, 3).Value = r.Percentage;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] ExportCompanyDirectory(IReadOnlyList<CompanyDirectoryRow> rows, bool includePiiContactData)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Direktori Perusahaan");

        ws.Cell(1, 1).Value = "DIREKTORI PERUSAHAAN & RECORD PIPELINE";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(2, 1).Value = includePiiContactData
            ? "Mode Ekspor: Lengkap (Termasuk Data Kontak PII — Akses Tersertifikasi Peraturan PDP Law UU 27/2022)"
            : "Mode Ekspor: Standar (Data Kontak PII Disamarkan / Masked)";
        ws.Cell(2, 1).Style.Font.Italic = true;

        var headers = new[] { "Nomor Register", "Nama Perusahaan", "Sektor Industri", "Sales Area", "Tahap", "Nama Kontak", "Telepon", "Email" };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E3A8A");
            cell.Style.Font.FontColor = XLColor.White;
        }

        int row = 5;
        foreach (var r in rows)
        {
            ws.Cell(row, 1).Value = r.Nomor;
            ws.Cell(row, 2).Value = r.NamaPerusahaan;
            ws.Cell(row, 3).Value = r.IndustryTypeName;
            ws.Cell(row, 4).Value = r.AreaName;
            ws.Cell(row, 5).Value = r.StageName;

            if (includePiiContactData)
            {
                ws.Cell(row, 6).Value = r.ContactName ?? "-";
                ws.Cell(row, 7).Value = r.ContactPhone ?? "-";
                ws.Cell(row, 8).Value = r.ContactEmail ?? "-";
            }
            else
            {
                ws.Cell(row, 6).Value = MaskPii(r.ContactName);
                ws.Cell(row, 7).Value = MaskPii(r.ContactPhone);
                ws.Cell(row, 8).Value = MaskPii(r.ContactEmail);
            }

            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private static string MaskPii(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "-";
        return value.Length <= 3 ? "***" : $"{value[..2]}***{value[^1..]}";
    }
}
