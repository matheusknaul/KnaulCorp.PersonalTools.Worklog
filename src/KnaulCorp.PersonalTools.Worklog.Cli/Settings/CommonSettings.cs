using System.ComponentModel;
using Spectre.Console.Cli;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Settings;
/// <summary>Opções comuns de interação e apresentação.</summary>
public class CommonSettings : CommandSettings
{
    #region Properties
    /// <summary>Desativa cores.</summary>
    [CommandOption("--no-color")]
    [Description("Exibir saída sem cores.")]
    public bool NoColor { get; init; }
    /// <summary>Desativa perguntas.</summary>
    [CommandOption("--non-interactive")]
    [Description("Executar sem perguntas.")]
    public bool NonInteractive { get; init; }
    #endregion
}
