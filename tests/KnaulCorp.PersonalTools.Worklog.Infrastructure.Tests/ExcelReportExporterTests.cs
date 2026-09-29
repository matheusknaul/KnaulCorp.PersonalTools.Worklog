using ClosedXML.Excel;
using System.IO.Compression;
using System.Xml.Linq;
using System.Globalization;
using Serilog;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
using KnaulCorp.PersonalTools.Worklog.Application.Reports;
using KnaulCorp.PersonalTools.Worklog.Infrastructure.Logging;
using KnaulCorp.PersonalTools.Worklog.Infrastructure.Reports;
namespace KnaulCorp.PersonalTools.Worklog.Infrastructure.Tests;
/// <summary>Confere tipos, totais e integridade do arquivo Excel gerado.</summary>
public sealed class ExcelReportExporterTests
{
    #region Methods
    [Fact]
    public async Task ExportAsync_WhenUserTextsAndLongDuration_ShouldPreserveNumericAndLiteralCells()
    {
        // Arrange
        string path = Path.Combine(Path.GetTempPath(), $"worklog-{Guid.NewGuid():N}.xlsx");
        using Serilog.Core.Logger logger = new LoggerConfiguration().CreateLogger();
        var exporter = new ExcelReportExporter(new(new SerilogAppLogger<ExcelReportExporter>(logger)));
        var time = new WorklogTime(TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo"), TimeProvider.System);
        var entry = new WorkEntryDto { Id = Guid.NewGuid(), StartedAt = DateTimeOffset.Parse("2026-09-18T12:00:00-03:00"), EndedAt = DateTimeOffset.Parse("2026-09-19T13:00:00-03:00"), Description = "=SUM(A1:A2)", Project = "+ERP", Activity = "0036347", Result = "@resultado com acentuação" };
        WorkReportDto report = new ReportBuilder(time).Build([entry], new(2026, 9, 18), new(2026, 9, 19));
        try
        {
            // Act
            await exporter.ExportAsync(report, path, false, default);
            byte[] bytes = await File.ReadAllBytesAsync(path);
            using var workbook = new XLWorkbook(new MemoryStream(bytes));
            // Assert
            IXLWorksheet sheet = workbook.Worksheet("Atividades");
            sheet.Cell(1, 10).GetString().Should().Be("Interrupção/Dispersão");
            sheet.Cell(2, 1).DataType.Should().Be(XLDataType.DateTime);
            GetNumericCell(bytes, "sheet1.xml", "D2").Should().BeApproximately(0.5, 1e-10);
            sheet.Cell(2, 4).Style.NumberFormat.Format.Should().Be("[h]:mm");
            sheet.Cell(2, 7).DataType.Should().Be(XLDataType.Text);
            sheet.Cell(2, 7).GetString().Should().Be("0036347");
            sheet.Cell(2, 12).HasFormula.Should().BeFalse();
            sheet.Cell(2, 12).GetString().Should().Be("=SUM(A1:A2)");
            sheet.Cell(2, 13).GetString().Should().Be("@resultado com acentuação");
            GetNumericCell(bytes, "sheet2.xml", "B6").Should().BeApproximately(25d / 24, 1e-10);
            workbook.Worksheet("Resumo").Cell(4, 2).GetDouble().Should().Be(1);
        }
        finally { File.Delete(path); }
    }
    [Fact]
    public async Task ExportAsync_WhenExistingFile_ShouldRefuseAndPreserveIt()
    {
        // Arrange
        string path = Path.Combine(Path.GetTempPath(), $"worklog-{Guid.NewGuid():N}.xlsx");
        await File.WriteAllTextAsync(path, "original");
        using Serilog.Core.Logger logger = new LoggerConfiguration().CreateLogger();
        var exporter = new ExcelReportExporter(new(new SerilogAppLogger<ExcelReportExporter>(logger)));
        var report = new WorkReportDto(new(2026, 9, 1), new(2026, 9, 30), "UTC", 0, 0, []);
        try
        {
            // Act
            Func<Task> action = () => exporter.ExportAsync(report, path, false, default);
            // Assert
            await action.Should().ThrowAsync<IOException>();
            (await File.ReadAllTextAsync(path)).Should().Be("original");
        }
        finally { File.Delete(path); }
    }
    [Fact]
    public async Task ExportAsync_WhenEmptyAndOverwrite_ShouldCreateValidWorkbook()
    {
        // Arrange
        string path = Path.Combine(Path.GetTempPath(), $"worklog-{Guid.NewGuid():N}.xlsx");
        await File.WriteAllTextAsync(path, "original");
        using Serilog.Core.Logger logger = new LoggerConfiguration().CreateLogger();
        var exporter = new ExcelReportExporter(new(new SerilogAppLogger<ExcelReportExporter>(logger)));
        var report = new WorkReportDto(new(2026, 9, 1), new(2026, 9, 30), "UTC", 0, 0, []);
        try
        {
            // Act
            await exporter.ExportAsync(report, path, true, default);
            byte[] bytes = await File.ReadAllBytesAsync(path);
            using var workbook = new XLWorkbook(new MemoryStream(bytes));
            // Assert
            workbook.Worksheets.Should().HaveCount(2);
            workbook.Worksheet("Atividades").LastRowUsed()!.RowNumber().Should().Be(1);
            GetNumericCell(bytes, "sheet2.xml", "B6").Should().Be(0);
        }
        finally { File.Delete(path); }
    }
    private static double GetNumericCell(byte[] bytes, string sheet, string address)
    {
        using var archive = new ZipArchive(new MemoryStream(bytes));
        using Stream stream = archive.GetEntry("xl/worksheets/" + sheet)!.Open();
        XDocument document = XDocument.Load(stream);
        XNamespace spreadsheet = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        XElement cell = document.Descendants(spreadsheet + "c").Single(value => (string?)value.Attribute("r") == address);
        ((string?)cell.Attribute("t") ?? "n").Should().Be("n");
        return double.Parse(cell.Element(spreadsheet + "v")!.Value, CultureInfo.InvariantCulture);
    }
    #endregion
}
