using Spectre.Console.Cli;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using KnaulCorp.PersonalTools.Worklog.Cli.Settings;
using KnaulCorp.PersonalTools.Worklog.Cli.Services;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands.EntryEdit;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Commands;
/// <summary>Executa o comando edit no terminal.</summary>
public sealed class EditCommand(IQueryMediator queries, ICommandMediator commands, WorklogTime time, WorklogUi ui) : AsyncCommand<EditSettings>
{
    #region Methods
    /// <inheritdoc />
    protected override async Task<int> ExecuteAsync(CommandContext context, EditSettings settings, CancellationToken cancellationToken)
    {
        IReadOnlyList<WorkEntryDto> entries = await queries.QueryAsync(new EntrySearchQuery(), cancellationToken);
        WorkEntryDto[] matches = entries.Where(entry => entry.Id.ToString().StartsWith(settings.Id, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (matches.Length != 1) throw new WorklogException(matches.Length == 0 ? "Registro não encontrado." : "Prefixo ambíguo. Informe o ID completo.");
        WorkEntryDto entry = matches[0];
        Guid id = await commands.SendAsync(new EntryEditCommand(entry.Id, entry.Version, settings.Start is null ? null : time.ParseInstant(settings.Start), settings.End is null ? null : time.ParseInstant(settings.End), EntryInput.GetPatch(settings, settings.Clear)), cancellationToken);
        ui.ShowSaved("Registro atualizado.", id);
        return 0;
    }
    #endregion
}
