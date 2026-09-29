using Spectre.Console.Cli;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using KnaulCorp.PersonalTools.Worklog.Cli.Settings;
using KnaulCorp.PersonalTools.Worklog.Cli.Services;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands.EntryAdd;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Commands;
/// <summary>Executa o comando add no terminal.</summary>
public sealed class AddCommand(EntryInput input, ICommandMediator commands, WorklogTime time, WorklogUi ui) : AsyncCommand<AddSettings>
{
    #region Methods
    /// <inheritdoc />
    protected override async Task<int> ExecuteAsync(CommandContext context, AddSettings settings, CancellationToken cancellationToken)
    {
        if (settings.Start is null || settings.End is null) throw new WorklogException("Informe --start e --end para um lançamento retroativo.");
        await input.CollectAsync(settings, cancellationToken);
        Guid id = await commands.SendAsync(new EntryAddCommand(EntryInput.GetDetails(settings).Normalize(), time.ParseInstant(settings.Start), time.ParseInstant(settings.End)), cancellationToken);
        ui.ShowSaved("Intervalo registrado.", id);
        return 0;
    }
    #endregion
}
