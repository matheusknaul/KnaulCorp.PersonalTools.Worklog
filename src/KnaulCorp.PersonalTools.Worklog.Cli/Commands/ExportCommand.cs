using Spectre.Console.Cli;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using KnaulCorp.PersonalTools.Worklog.Cli.Settings;
using KnaulCorp.PersonalTools.Worklog.Cli.Services;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
using KnaulCorp.PersonalTools.Worklog.Application.Reports;
using KnaulCorp.PersonalTools.Worklog.Application.Common.Interfaces;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Commands;
/// <summary>Executa Export na interface de terminal.</summary>
public sealed class ExportCommand(IQueryMediator queries, PeriodInput periods, ReportBuilder reports, IReportExporter exporter, WorklogUi ui) : AsyncCommand<ExportSettings>
{
    #region Methods
    /// <inheritdoc />
    protected override async Task<int> ExecuteAsync(CommandContext context, ExportSettings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Output)) throw new WorklogException("Informe --output arquivo.xlsx.");
        var period = periods.GetPeriod(settings);
        IReadOnlyList<WorkEntryDto> entries = await queries.QueryAsync(period.Query, cancellationToken);
        WorkReportDto report = reports.Build(entries, period.From, period.To);
        try { await exporter.ExportAsync(report, settings.Output, settings.Overwrite, cancellationToken); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            await Console.Error.WriteLineAsync("Não foi possível salvar o Excel. Confira o destino, as permissões e se o arquivo está aberto; use --overwrite para substituir um arquivo existente.");
            return 4;
        }
        ui.ShowMessage($"Excel salvo em {Path.GetFullPath(settings.Output)}. Registros: {report.ClosedCount}; abertos excluídos: {report.OpenCount}.");
        if (report.ClosedCount == 0) ui.ShowMessage("Período sem atividades encerradas; planilha com cabeçalhos e resumo zerado.");
        return 0;
    }
    #endregion
}
