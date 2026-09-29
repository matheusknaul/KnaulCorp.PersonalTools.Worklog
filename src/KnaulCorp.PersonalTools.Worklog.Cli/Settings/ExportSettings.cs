using System.ComponentModel;
using Spectre.Console.Cli;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Settings;
/// <summary>Destino e política de exportação.</summary>
public sealed class ExportSettings : PeriodSettings
{
    #region Properties
    /// <summary>Caminho do arquivo Excel.</summary>
    [CommandOption("--output <FILE>")]
    [Description("Caminho obrigatório do arquivo .xlsx.")]
    public string? Output { get; init; }
    /// <summary>Autoriza substituição do destino.</summary>
    [CommandOption("--overwrite")]
    [Description("Substituir arquivo existente.")]
    public bool Overwrite { get; init; }
    #endregion
}
