using Spectre.Console.Cli;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using KnaulCorp.PersonalTools.Worklog.Cli.Settings;
using KnaulCorp.PersonalTools.Worklog.Cli.Services;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands.EntryStart;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Commands;
/// <summary>Executa o comando start no terminal.</summary>
public sealed class StartCommand(EntryInput input, ICommandMediator commands, WorklogTime time, WorklogUi ui) : AsyncCommand<StartSettings>
{
    #region Methods
    /// <inheritdoc />
    protected override async Task<int> ExecuteAsync(CommandContext context, StartSettings settings, CancellationToken cancellationToken)
    {
        await input.CollectAsync(settings, cancellationToken);
        Guid id = await commands.SendAsync(new EntryStartCommand(EntryInput.GetDetails(settings).Normalize(), settings.At is null ? time.Now : time.ParseInstant(settings.At)), cancellationToken);
        ui.ShowSaved("Atividade iniciada. Use worklog stop ao terminar.", id);
        return 0;
    }
    #endregion
}
