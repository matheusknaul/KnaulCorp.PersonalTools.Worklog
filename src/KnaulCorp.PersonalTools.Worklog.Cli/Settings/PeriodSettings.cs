using System.ComponentModel;
using Spectre.Console.Cli;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Settings;
/// <summary>Opções de período e filtros de consulta.</summary>
public class PeriodSettings : CommonSettings
{
    #region Properties
    /// <summary>Data inicial inclusiva, YYYY-MM-DD.</summary>
    [CommandOption("--from <DATE>")]
    [Description("Data inicial inclusiva, YYYY-MM-DD.")]
    public string? From { get; init; }
    /// <summary>Data final inclusiva, YYYY-MM-DD.</summary>
    [CommandOption("--to <DATE>")]
    [Description("Data final inclusiva, YYYY-MM-DD.")]
    public string? To { get; init; }
    /// <summary>Filtrar projeto exato.</summary>
    [CommandOption("--project <VALUE>")]
    [Description("Filtrar projeto exato.")]
    public string? Project { get; init; }
    /// <summary>Filtrar tipo exato.</summary>
    [CommandOption("--type <VALUE>")]
    [Description("Filtrar tipo exato.")]
    public string? Type { get; init; }
    /// <summary>Filtrar código externo exato.</summary>
    [CommandOption("--activity <VALUE>")]
    [Description("Filtrar código externo exato.")]
    public string? Activity { get; init; }
    /// <summary>Usa somente a data atual.</summary>
    [CommandOption("--today")]
    [Description("Consultar o dia atual.")]
    public bool Today { get; init; }
    #endregion
}
