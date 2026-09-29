using KnaulCorp.PersonalTools.Worklog.Domain.ValueObjects;
namespace KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands;
/// <summary>Alterações parciais, incluindo a limpeza explícita de campos.</summary>
public sealed record EntryPatch(WorkDetails Details, IReadOnlySet<string> Supplied, IReadOnlySet<string> Cleared)
{
    #region Methods
    /// <summary>Combina as alterações informadas com os dados existentes.</summary>
    public WorkDetails Apply(WorkDetails current) => new(
        Supplied.Contains("description") ? Details.Description : current.Description,
        Cleared.Contains("project") ? null : Supplied.Contains("project") ? Details.Project : current.Project,
        Cleared.Contains("epic") ? null : Supplied.Contains("epic") ? Details.Epic : current.Epic,
        Cleared.Contains("activity") ? null : Supplied.Contains("activity") ? Details.Activity : current.Activity,
        Cleared.Contains("type") ? null : Supplied.Contains("type") ? Details.Type : current.Type,
        Cleared.Contains("planned") ? null : Supplied.Contains("planned") ? Details.Planned : current.Planned,
        Cleared.Contains("interruption") ? null : Supplied.Contains("interruption") ? Details.Interruption : current.Interruption,
        Cleared.Contains("origin") ? null : Supplied.Contains("origin") ? Details.Origin : current.Origin,
        Cleared.Contains("result") ? null : Supplied.Contains("result") ? Details.Result : current.Result);
    #endregion
}
