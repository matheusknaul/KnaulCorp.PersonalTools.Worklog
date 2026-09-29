using ClosedXML.Excel;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Common.Interfaces;
using KnaulCorp.PersonalTools.Worklog.Application.Reports;
namespace KnaulCorp.PersonalTools.Worklog.Infrastructure.Reports;
/// <summary>Exporta durações numéricas e textos literais para uma planilha Excel.</summary>
public sealed class ExcelReportExporter(OperationRunner<ExcelReportExporter> operations) : IReportExporter
{
    #region Methods
    /// <inheritdoc />
    public async Task ExportAsync(WorkReportDto report, string path, bool overwrite, CancellationToken cancellationToken)
    {
        await operations.RunAsync("Export", async () =>
        {
            string destination = Path.GetFullPath(path);
            if (!string.Equals(Path.GetExtension(destination), ".xlsx", StringComparison.OrdinalIgnoreCase)) throw new IOException("O destino deve ter extensão .xlsx.");
            if (File.Exists(destination) && !overwrite) throw new IOException("O arquivo já existe. Use --overwrite para substituí-lo.");
            using var workbook = new XLWorkbook();
            IXLWorksheet sheet = workbook.AddWorksheet("Atividades");
            string[] headers = ["Data", "Inicio", "Fim", "Tempo", "Projeto", "Epic", "Atividade", "Tipo", "Planejado", "Interrupção/Dispersão", "Origem", "Descrição de trabalho", "Resultado"];
            for (int column = 0; column < headers.Length; column++) sheet.Cell(1, column + 1).Value = headers[column];
            int row = 2;
            foreach (ReportSegmentDto segment in report.Segments)
            {
                sheet.Cell(row, 1).Value = segment.Date.ToDateTime(TimeOnly.MinValue);
                sheet.Cell(row, 2).Value = segment.Start.TimeOfDay.TotalDays;
                sheet.Cell(row, 3).Value = segment.End.TimeOfDay.TotalDays;
                sheet.Cell(row, 4).Value = segment.Duration.TotalDays;
                string?[] values = [segment.Entry.Project, segment.Entry.Epic, segment.Entry.Activity, segment.Entry.Type, segment.Entry.Planned is null ? null : segment.Entry.Planned.Value ? "Sim" : "Não", segment.Entry.Interruption ?? "N/A", segment.Entry.Origin, segment.Entry.Description, segment.Entry.Result];
                for (int column = 0; column < values.Length; column++) sheet.Cell(row, column + 5).Value = values[column] ?? "";
                row++;
            }
            sheet.Column(1).Style.DateFormat.Format = "dd/mm/yyyy";
            sheet.Columns(2, 3).Style.NumberFormat.Format = "hh:mm:ss";
            sheet.Column(4).Style.NumberFormat.Format = "[h]:mm";
            sheet.Column(7).Style.NumberFormat.Format = "@";
            sheet.Columns(1, 4).Width = 14;
            sheet.Columns(5, 11).Width = 24;
            sheet.Columns(12, 13).Width = 55;
            sheet.Columns(5, 13).Style.Alignment.WrapText = true;
            sheet.Range(1, 1, Math.Max(1, row - 1), 13).SetAutoFilter();
            StyleHeader(sheet, 13);
            IXLWorksheet summary = workbook.AddWorksheet("Resumo");
            summary.Cell(1, 1).Value = "Resumo de trabalho";
            summary.Cell(1, 2).Value = "Valor";
            summary.Cell(2, 1).Value = "Período";
            summary.Cell(2, 2).Value = $"{report.From:dd/MM/yyyy} a {report.To:dd/MM/yyyy}";
            summary.Cell(3, 1).Value = "Fuso"; summary.Cell(3, 2).Value = report.TimeZone;
            summary.Cell(4, 1).Value = "Registros encerrados"; summary.Cell(4, 2).Value = report.ClosedCount;
            summary.Cell(5, 1).Value = "Abertos excluídos"; summary.Cell(5, 2).Value = report.OpenCount;
            summary.Cell(6, 1).Value = "Tempo total"; summary.Cell(6, 2).Value = report.Total.TotalDays;
            summary.Cell(6, 2).Style.NumberFormat.Format = "[h]:mm";
            summary.Cell(8, 1).Value = "Fim 00:00:00 em trechos diários indica o início do dia seguinte. Tempo preserva segundos; a exibição mostra horas e minutos.";
            summary.Range(8, 1, 9, 2).Merge().Style.Alignment.WrapText = true;
            int summaryRow = 11;
            AddGroups(summary, ref summaryRow, "Dia", report.Segments.GroupBy(segment => segment.Date.ToString("dd/MM/yyyy")));
            AddGroups(summary, ref summaryRow, "Projeto", report.Segments.GroupBy(segment => segment.Entry.Project ?? "Não informado"));
            AddGroups(summary, ref summaryRow, "Tipo", report.Segments.GroupBy(segment => segment.Entry.Type ?? "Não informado"));
            summary.Column(1).Width = 48; summary.Column(2).Width = 40;
            StyleHeader(summary, 2);
            using var buffer = new MemoryStream();
            workbook.SaveAs(buffer);
            buffer.Position = 0;
            string temporary = Path.Combine(Path.GetDirectoryName(destination)!, $".worklog-{Guid.NewGuid():N}.tmp");
            try
            {
                await using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536, FileOptions.Asynchronous))
                {
                    await buffer.CopyToAsync(output, cancellationToken);
                    await output.FlushAsync(cancellationToken);
                }
                cancellationToken.ThrowIfCancellationRequested();
                File.Move(temporary, destination, overwrite);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
            return 0;
        });
    }
    private static void StyleHeader(IXLWorksheet sheet, int columns)
    {
        sheet.Range(1, 1, 1, columns).Style.Fill.BackgroundColor = XLColor.FromHtml("#E98B24");
        sheet.Range(1, 1, 1, columns).Style.Font.Bold = true;
        sheet.Row(1).Height = 28;
        sheet.SheetView.FreezeRows(1);
    }
    private static void AddGroups(IXLWorksheet sheet, ref int row, string label, IEnumerable<IGrouping<string, ReportSegmentDto>> groups)
    {
        sheet.Cell(row, 1).Value = label; sheet.Cell(row, 2).Value = "Tempo";
        sheet.Range(row, 1, row, 2).Style.Font.Bold = true;
        row++;
        foreach (IGrouping<string, ReportSegmentDto> group in groups)
        {
            sheet.Cell(row, 1).Value = group.Key;
            sheet.Cell(row, 2).Value = TimeSpan.FromTicks(group.Sum(segment => segment.Duration.Ticks)).TotalDays;
            sheet.Cell(row, 2).Style.NumberFormat.Format = "[h]:mm";
            row++;
        }
        row++;
    }
    #endregion
}
