using LiteBus.Queries.Abstractions;
using Spectre.Console;
using KnaulCorp.PersonalTools.Worklog.Cli.Settings;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
using KnaulCorp.PersonalTools.Worklog.Domain.ValueObjects;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Services;
/// <summary>Coleta e interpreta campos da interface de terminal.</summary>
public sealed class EntryInput(IAnsiConsole console, IQueryMediator queries, OptionalInput optionalInput)
{
    #region Methods
    /// <summary>Determina se perguntas podem ser feitas.</summary>
    public static bool CanPrompt(CommonSettings settings) => !settings.NonInteractive && !Console.IsInputRedirected && !Console.IsOutputRedirected;
    /// <summary>Coleta descrição e contexto opcionais de um novo registro.</summary>
    public async Task CollectAsync(EntrySettings settings, CancellationToken cancellationToken)
    {
        if (settings.Description is not null || !CanPrompt(settings)) return;
        settings.Description = await console.PromptAsync(new TextPrompt<string>("[orange1]O que você vai fazer?[/]").ValidationErrorMessage("Informe uma descrição."), cancellationToken);
        string choice = await console.PromptAsync(new SelectionPrompt<string>().Title("Projeto e classificação").AddChoices("Pular por enquanto", "Complementar agora"), cancellationToken);
        if (choice == "Pular por enquanto") return;
        IReadOnlyList<WorkEntryDto> history = await queries.QueryAsync(new EntrySearchQuery(), cancellationToken);
        settings.Project ??= await optionalInput.AskAsync("Projeto", history.Select(entry => entry.Project), cancellationToken);
        settings.Epic ??= await optionalInput.AskAsync("Epic", history.Select(entry => entry.Epic), cancellationToken);
        settings.Activity ??= await optionalInput.AskAsync("Atividade (código externo)", [], cancellationToken);
        settings.Type ??= await optionalInput.AskAsync("Tipo", history.Select(entry => entry.Type).Concat(["Desenvolvimento", "Revisão de código", "Reunião", "Suporte", "Estudo", "Interrupção", "Dispersão"]), cancellationToken);
        if (settings.Planned is null)
        {
            string planned = await console.PromptAsync(new SelectionPrompt<string>().Title("Planejado?").AddChoices("Não informado", "Sim", "Não"), cancellationToken);
            settings.Planned = planned == "Sim" ? "yes" : planned == "Não" ? "no" : null;
        }
        settings.Interruption ??= await optionalInput.AskAsync("Interrupção/Dispersão", [], cancellationToken);
        settings.Origin ??= await optionalInput.AskAsync("Origem", history.Select(entry => entry.Origin).Concat(["Analista", "Própria", "Equipe", "Gestor"]), cancellationToken);
    }
    /// <summary>Coleta resultado opcional ao encerrar.</summary>
    public async Task CollectResultAsync(EntrySettings settings, CancellationToken cancellationToken)
    {
        if (settings.Result is null && CanPrompt(settings)) settings.Result = await optionalInput.AskAsync("Resultado", [], cancellationToken);
    }
    /// <summary>Converte opções em dados de domínio.</summary>
    public static WorkDetails GetDetails(EntrySettings settings) => new(settings.Description ?? "", settings.Project, settings.Epic, settings.Activity, settings.Type, ParsePlanned(settings.Planned), settings.Interruption, settings.Origin, settings.Result);
    /// <summary>Constrói alteração parcial e valida os nomes dos campos a limpar.</summary>
    public static EntryPatch GetPatch(EntrySettings settings, string[]? clear = null)
    {
        var supplied = new HashSet<string>(StringComparer.Ordinal);
        if (settings.Description is not null) supplied.Add("description");
        if (settings.Project is not null) supplied.Add("project");
        if (settings.Epic is not null) supplied.Add("epic");
        if (settings.Activity is not null) supplied.Add("activity");
        if (settings.Type is not null) supplied.Add("type");
        if (settings.Planned is not null) supplied.Add("planned");
        if (settings.Interruption is not null) supplied.Add("interruption");
        if (settings.Origin is not null) supplied.Add("origin");
        if (settings.Result is not null) supplied.Add("result");
        var cleared = new HashSet<string>(clear ?? [], StringComparer.Ordinal);
        string[] optional = ["project", "epic", "activity", "type", "planned", "interruption", "origin", "result"];
        if (cleared.Any(field => !optional.Contains(field))) throw new WorklogException("--clear aceita apenas nomes de campos opcionais, como project, epic ou result.");
        if (cleared.Overlaps(supplied)) throw new WorklogException("Um campo não pode ser definido e limpo na mesma chamada.");
        return new(GetDetails(settings), supplied, cleared);
    }
    private static bool? ParsePlanned(string? value) => value?.ToLowerInvariant() switch { null => null, "yes" => true, "no" => false, _ => throw new WorklogException("--planned deve ser yes ou no.") };
    #endregion
}
