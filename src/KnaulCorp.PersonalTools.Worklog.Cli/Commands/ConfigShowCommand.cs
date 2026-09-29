using Spectre.Console.Cli;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using KnaulCorp.PersonalTools.Worklog.Cli.Settings;
using KnaulCorp.PersonalTools.Worklog.Cli.Services;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Commands;
/// <summary>Executa ConfigShow na interface de terminal.</summary>
public sealed class ConfigShowCommand(IConfiguration configuration, WorklogTime time, WorklogUi ui) : AsyncCommand<CommonSettings>
{
    #region Methods
    /// <inheritdoc />
    protected override async Task<int> ExecuteAsync(CommandContext context, CommonSettings settings, CancellationToken cancellationToken)
    {
        ui.ShowMessage($"Fuso: {time.Zone.Id}");
        ui.ShowMessage($"Conexão: {(string.IsNullOrWhiteSpace(configuration.GetConnectionString("Worklog")) ? "não configurada" : "configurada (conteúdo oculto)")}");
        ui.ShowMessage("Schema: worklog | Idioma: pt-BR");
        await Task.CompletedTask;
        return 0;
    }
    #endregion
}
