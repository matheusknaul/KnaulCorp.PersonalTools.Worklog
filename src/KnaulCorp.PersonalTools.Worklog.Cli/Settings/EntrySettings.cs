using System.ComponentModel;
using Spectre.Console.Cli;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Settings;
/// <summary>Campos comuns de registro de atividades.</summary>
public class EntrySettings : CommonSettings
{
    #region Properties
    /// <summary>Campo Description informado pelo usuário.</summary>
    [CommandOption("--description <VALUE>")]
    [Description("Descrição do trabalho; obrigatória em novos registros.")]
    public string? Description { get; set; }
    /// <summary>Campo Project informado pelo usuário.</summary>
    [CommandOption("--project <VALUE>")]
    [Description("Projeto (opcional).")]
    public string? Project { get; set; }
    /// <summary>Campo Epic informado pelo usuário.</summary>
    [CommandOption("--epic <VALUE>")]
    [Description("Epic (opcional).")]
    public string? Epic { get; set; }
    /// <summary>Campo Activity informado pelo usuário.</summary>
    [CommandOption("--activity <VALUE>")]
    [Description("Código externo da atividade (opcional).")]
    public string? Activity { get; set; }
    /// <summary>Campo Type informado pelo usuário.</summary>
    [CommandOption("--type <VALUE>")]
    [Description("Tipo da atividade (opcional).")]
    public string? Type { get; set; }
    /// <summary>Campo Planned informado pelo usuário.</summary>
    [CommandOption("--planned <VALUE>")]
    [Description("Planejado: yes ou no.")]
    public string? Planned { get; set; }
    /// <summary>Campo Interruption informado pelo usuário.</summary>
    [CommandOption("--interruption <VALUE>")]
    [Description("Motivo de interrupção ou dispersão (opcional).")]
    public string? Interruption { get; set; }
    /// <summary>Campo Origin informado pelo usuário.</summary>
    [CommandOption("--origin <VALUE>")]
    [Description("Origem da demanda (opcional).")]
    public string? Origin { get; set; }
    /// <summary>Campo Result informado pelo usuário.</summary>
    [CommandOption("--result <VALUE>")]
    [Description("Resultado do trabalho (opcional).")]
    public string? Result { get; set; }
    #endregion
}
