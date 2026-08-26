using ClosedXML.Excel;
using Shouldly;
using Simando.Application.Reports;
using Simando.Infrastructure.Reports;

namespace Simando.Integration.Tests.Reports;

public class ExcelExportServiceTests
{
    private readonly ExcelExportService _service = new();

    [Fact(DisplayName = "ExportFunnelReport generates non-empty Excel workbook")]
    public void ExportFunnelReport_GeneratesWorkbook()
    {
        var dto = new FunnelReportDto(
            [new FunnelStageRow(1, "Direktori", 10, 100.0, 1.2)],
            10,
            10.0
        );

        var bytes = _service.ExportFunnelReport(dto);

        bytes.ShouldNotBeNull();
        bytes.Length.ShouldBeGreaterThan(0);

        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);

        workbook.Worksheets.Count.ShouldBe(1);
        var ws = workbook.Worksheet("Corong Penjualan");
        ws.Cell(1, 1).Value.ToString().ShouldContain("CORONG PENJUALAN");
    }

    [Fact(DisplayName = "ExportCompanyDirectory masks PII when includePiiContactData is false")]
    public void ExportCompanyDirectory_MasksPiiWhenFalse()
    {
        var rows = new List<CompanyDirectoryRow>
        {
            new(Guid.NewGuid(), "0101", "PT Test", "Jl. Test", "Manufaktur", "Surabaya", 1, "Direktori", "Budi Santoso", "08123456789", "budi@test.com")
        };

        var bytes = _service.ExportCompanyDirectory(rows, includePiiContactData: false);

        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);

        var ws = workbook.Worksheet("Direktori Perusahaan");
        ws.Cell(5, 6).Value.ToString().ShouldContain("***"); // ContactName masked
        ws.Cell(5, 7).Value.ToString().ShouldContain("***"); // ContactPhone masked
    }

    [Fact(DisplayName = "ExportCompanyDirectory includes raw PII when includePiiContactData is true")]
    public void ExportCompanyDirectory_IncludesPiiWhenTrue()
    {
        var rows = new List<CompanyDirectoryRow>
        {
            new(Guid.NewGuid(), "0101", "PT Test", "Jl. Test", "Manufaktur", "Surabaya", 1, "Direktori", "Budi Santoso", "08123456789", "budi@test.com")
        };

        var bytes = _service.ExportCompanyDirectory(rows, includePiiContactData: true);

        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);

        var ws = workbook.Worksheet("Direktori Perusahaan");
        ws.Cell(5, 6).Value.ToString().ShouldBe("Budi Santoso");
        ws.Cell(5, 7).Value.ToString().ShouldBe("08123456789");
    }
}
