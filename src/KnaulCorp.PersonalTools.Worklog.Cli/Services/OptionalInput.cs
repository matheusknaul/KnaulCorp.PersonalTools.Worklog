using Spectre.Console;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Services;
/// <summary>Oferece sugestões sem restringir o usuário a um catálogo fechado.</summary>
public sealed class OptionalInput(IAnsiConsole console)
{
    #region Methods
    /// <summary>Permite pular, reutilizar um valor ou digitar livremente.</summary>
    public async Task<string?> AskAsync(string label, IEnumerable<string?> suggestions, CancellationToken cancellationToken)
    {
        string[] existing = suggestions.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!).Distinct().Take(30).ToArray();
        if (existing.Length > 0)
        {
            PromptChoice[] choices = [new("Pular por enquanto", null), new("Digitar outro valor", null, true), .. existing.Select(value => new PromptChoice(value, value))];
            var selection = new SelectionPrompt<PromptChoice>().Title(Markup.Escape(label)).UseConverter(choice => Markup.Escape(choice.Label)).MoreChoicesText("[grey](use as setas para ver mais opções)[/]").AddChoices(choices);
            PromptChoice selected = await console.PromptAsync(selection, cancellationToken);
            if (!selected.Custom) return selected.Value;
        }
        string answer = await console.PromptAsync(new TextPrompt<string>($"{Markup.Escape(label)} [grey](Enter para pular)[/]:").AllowEmpty(), cancellationToken);
        return string.IsNullOrWhiteSpace(answer) ? null : answer;
    }
    #endregion
}
