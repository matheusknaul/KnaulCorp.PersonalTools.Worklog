using System.ComponentModel;
using Spectre.Console.Cli;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Settings;
/// <summary>Opções para Add.</summary>
public sealed class AddSettings : EntrySettings
{
    #region Properties
    /// <summary>Horário ISO explícito.</summary>
    [CommandOption("--start <ISO>")]
    [Description("Data e hora ISO 8601; sem offset usa o fuso configurado.")]
    public string? Start { get; init; }
    /// <summary>Horário ISO explícito.</summary>
    [CommandOption("--end <ISO>")]
    [Description("Data e hora ISO 8601; sem offset usa o fuso configurado.")]
    public string? End { get; init; }
    #endregion
}
