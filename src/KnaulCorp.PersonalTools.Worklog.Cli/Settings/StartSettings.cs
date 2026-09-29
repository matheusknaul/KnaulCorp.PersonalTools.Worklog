using System.ComponentModel;
using Spectre.Console.Cli;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Settings;
/// <summary>Opções para Start.</summary>
public sealed class StartSettings : EntrySettings
{
    #region Properties
    /// <summary>Horário ISO explícito.</summary>
    [CommandOption("--at <ISO>")]
    [Description("Data e hora ISO 8601; sem offset usa o fuso configurado.")]
    public string? At { get; init; }
    #endregion
}
