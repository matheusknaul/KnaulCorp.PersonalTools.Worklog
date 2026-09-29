using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
namespace KnaulCorp.PersonalTools.Worklog.Domain.ValueObjects;
/// <summary>Dados descritivos de uma atividade.</summary>
public sealed record WorkDetails(string Description, string? Project = null, string? Epic = null, string? Activity = null, string? Type = null, bool? Planned = null, string? Interruption = null, string? Origin = null, string? Result = null)
{
    #region Methods
    /// <summary>Normaliza e valida os campos de trabalho.</summary>
    public WorkDetails Normalize()
    {
        string description = Clean(Description, 4000) ?? throw new WorklogException("Informe uma descrição de trabalho.");
        return this with { Description = description, Project = Clean(Project, 200), Epic = Clean(Epic, 200), Activity = Clean(Activity, 100), Type = Clean(Type, 200), Interruption = Clean(Interruption, 4000), Origin = Clean(Origin, 200), Result = Clean(Result, 4000) };
    }
    private static string? Clean(string? value, int maximum)
    {
        string? cleaned = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (cleaned?.Length > maximum) throw new WorklogException($"O texto excede o limite de {maximum} caracteres.");
        return cleaned;
    }
    #endregion
}
