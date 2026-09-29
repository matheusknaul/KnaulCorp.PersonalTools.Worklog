using System.ComponentModel;
using Spectre.Console.Cli;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Settings;
/// <summary>Opções para Edit.</summary>
public sealed class EditSettings : EntrySettings
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
    /// <summary>Identificador completo ou prefixo não ambíguo.</summary>
    [CommandArgument(0, "<id>")]
    public string Id { get; init; } = "";
    /// <summary>Campos opcionais a limpar.</summary>
    [CommandOption("--clear <FIELD>")]
    [Description("Limpar campo opcional; pode ser repetida.")]
    public string[] Clear { get; init; } = [];
    #endregion
}
