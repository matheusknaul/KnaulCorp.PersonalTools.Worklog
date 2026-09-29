using Spectre.Console.Cli;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using KnaulCorp.PersonalTools.Worklog.Cli.Settings;
using KnaulCorp.PersonalTools.Worklog.Cli.Services;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands.EntryStop;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Commands;
/// <summary>Executa o comando stop no terminal.</summary>
public sealed class StopCommand(EntryInput input, IQueryMediator queries, ICommandMediator commands, WorklogTime time, WorklogUi ui) : AsyncCommand<StopSettings>
{
    #region Methods
    /// <inheritdoc />
    protected override async Task<int> ExecuteAsync(CommandContext context, StopSettings settings, CancellationToken cancellationToken)
    {
        IReadOnlyList<WorkEntryDto> active = await queries.QueryAsync(new EntrySearchQuery(ActiveOnly: true), cancellationToken);
        WorkEntryDto entry = active.SingleOrDefault() ?? throw new WorklogException("Nenhuma atividade aberta. Use worklog start.");
        await input.CollectResultAsync(settings, cancellationToken);
        Guid id = await commands.SendAsync(new EntryStopCommand(entry.Id, entry.Version, settings.At is null ? time.Now : time.ParseInstant(settings.At), EntryInput.GetPatch(settings)), cancellationToken);
        ui.ShowSaved("Atividade encerrada. Use worklog report --today para consultar o dia.", id);
        return 0;
    }
    #endregion
}
