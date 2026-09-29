using Spectre.Console.Cli;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using KnaulCorp.PersonalTools.Worklog.Cli.Settings;
using KnaulCorp.PersonalTools.Worklog.Cli.Services;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;

namespace KnaulCorp.PersonalTools.Worklog.Cli.Commands;
/// <summary>Executa List na interface de terminal.</summary>
public sealed class ListCommand(IQueryMediator queries, PeriodInput periods, WorklogUi ui) : AsyncCommand<PeriodSettings>
{
    #region Methods
    /// <inheritdoc />
    protected override async Task<int> ExecuteAsync(CommandContext context, PeriodSettings settings, CancellationToken cancellationToken)
    {
        var period = periods.GetPeriod(settings);
        IReadOnlyList<WorkEntryDto> entries = await queries.QueryAsync(period.Query, cancellationToken);
        ui.ShowEntries(entries);
        return 0;
    }
    #endregion
}
