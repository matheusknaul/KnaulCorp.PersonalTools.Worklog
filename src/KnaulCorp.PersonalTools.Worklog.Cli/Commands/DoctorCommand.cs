using Spectre.Console.Cli;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using KnaulCorp.PersonalTools.Worklog.Cli.Settings;
using KnaulCorp.PersonalTools.Worklog.Cli.Services;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
using KnaulCorp.PersonalTools.Worklog.Infrastructure.Persistence;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Commands;
/// <summary>Executa Doctor na interface de terminal.</summary>
public sealed class DoctorCommand(DatabaseMaintenance database, WorklogUi ui) : AsyncCommand<CommonSettings>
{
    #region Methods
    /// <inheritdoc />
    protected override async Task<int> ExecuteAsync(CommandContext context, CommonSettings settings, CancellationToken cancellationToken)
    {
        ui.ShowMessage(await database.DiagnoseAsync(cancellationToken));
        return 0;
    }
    #endregion
}
