namespace KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
/// <summary>Projeção de leitura de um intervalo de trabalho.</summary>
public sealed class WorkEntryDto
{
    #region Properties
    /// <summary>Valor de Id.</summary>
    public Guid Id { get; init; }
    /// <summary>Valor de Version.</summary>
    public Guid Version { get; init; }
    /// <summary>Valor de StartedAt.</summary>
    public DateTimeOffset StartedAt { get; init; }
    /// <summary>Valor de EndedAt.</summary>
    public DateTimeOffset? EndedAt { get; init; }
    /// <summary>Valor de Description.</summary>
    public string Description { get; init; } = "";
    /// <summary>Valor de Project.</summary>
    public string? Project { get; init; }
    /// <summary>Valor de Epic.</summary>
    public string? Epic { get; init; }
    /// <summary>Valor de Activity.</summary>
    public string? Activity { get; init; }
    /// <summary>Valor de Type.</summary>
    public string? Type { get; init; }
    /// <summary>Valor de Planned.</summary>
    public bool? Planned { get; init; }
    /// <summary>Valor de Interruption.</summary>
    public string? Interruption { get; init; }
    /// <summary>Valor de Origin.</summary>
    public string? Origin { get; init; }
    /// <summary>Valor de Result.</summary>
    public string? Result { get; init; }
    #endregion
}
