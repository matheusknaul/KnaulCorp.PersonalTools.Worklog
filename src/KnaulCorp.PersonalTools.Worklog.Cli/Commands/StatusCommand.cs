using Spectre.Console.Cli;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using KnaulCorp.PersonalTools.Worklog.Cli.Settings;
using KnaulCorp.PersonalTools.Worklog.Cli.Services;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;

namespace KnaulCorp.PersonalTools.Worklog.Cli.Commands;
/// <summary>Executa Status na interface de terminal.</summary>
public sealed class StatusCommand(IQueryMediator queries, WorklogUi ui) : AsyncCommand<CommonSettings>
{
    #region Methods
    /// <inheritdoc />
    protected override async Task<int> ExecuteAsync(CommandContext context, CommonSettings settings, CancellationToken cancellationToken)
    {
        IReadOnlyList<WorkEntryDto> entries = await queries.QueryAsync(new EntrySearchQuery(ActiveOnly: true), cancellationToken);
        ui.ShowStatus(entries.SingleOrDefault());
        return 0;
    }
    #endregion
}
