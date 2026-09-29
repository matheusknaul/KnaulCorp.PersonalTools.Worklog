namespace KnaulCorp.PersonalTools.Worklog.Cli.Services;
/// <summary>Opção de seleção com distinção entre valor existente e entrada livre.</summary>
public sealed record PromptChoice(string Label, string? Value, bool Custom = false);
