using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
using KnaulCorp.PersonalTools.Worklog.Domain.ValueObjects;
namespace KnaulCorp.PersonalTools.Worklog.Domain.Entities;
/// <summary>Intervalo contínuo de trabalho com controle de edição concorrente.</summary>
public sealed class WorkEntry
{
    #region Properties
    /// <summary>Identificador interno.</summary>
    public Guid Id { get; private set; }
    /// <summary>Versão usada para detectar alterações concorrentes.</summary>
    public Guid Version { get; private set; }
    /// <summary>Início UTC.</summary>
    public DateTimeOffset StartedAt { get; private set; }
    /// <summary>Fim UTC ou ausência enquanto aberta.</summary>
    public DateTimeOffset? EndedAt { get; private set; }
    /// <summary>Campo Description do registro.</summary>
    public string Description { get; private set; } = "";
    /// <summary>Campo Project do registro.</summary>
    public string? Project { get; private set; }
    /// <summary>Campo Epic do registro.</summary>
    public string? Epic { get; private set; }
    /// <summary>Campo Activity do registro.</summary>
    public string? Activity { get; private set; }
    /// <summary>Campo Type do registro.</summary>
    public string? Type { get; private set; }
    /// <summary>Campo Planned do registro.</summary>
    public bool? Planned { get; private set; }
    /// <summary>Campo Interruption do registro.</summary>
    public string? Interruption { get; private set; }
    /// <summary>Campo Origin do registro.</summary>
    public string? Origin { get; private set; }
    /// <summary>Campo Result do registro.</summary>
    public string? Result { get; private set; }
    #endregion
    #region Constructors
    private WorkEntry() { }
    #endregion
    #region Methods
    /// <summary>Cria um intervalo válido.</summary>
    public static WorkEntry Create(DateTimeOffset start, DateTimeOffset? end, WorkDetails details, DateTimeOffset now)
    {
        var entry = new WorkEntry { Id = Guid.NewGuid() };
        entry.Update(start, end, details, now);
        return entry;
    }
    /// <summary>Atualiza dados sem permitir a reabertura de intervalos encerrados.</summary>
    public void Update(DateTimeOffset start, DateTimeOffset? end, WorkDetails details, DateTimeOffset now)
    {
        start = Truncate(start);
        end = end.HasValue ? Truncate(end.Value) : null;
        if (EndedAt.HasValue && !end.HasValue) throw new WorklogException("Um registro encerrado não pode ser reaberto.");
        if (end <= start) throw new WorklogException("O fim deve ser posterior ao início.");
        if (start > now || end > now) throw new WorklogException("Não é permitido registrar horários futuros.");
        WorkDetails normalized = details.Normalize();
        StartedAt = start;
        EndedAt = end;
        Version = Guid.NewGuid();
        Description = normalized.Description;
        Project = normalized.Project;
        Epic = normalized.Epic;
        Activity = normalized.Activity;
        Type = normalized.Type;
        Planned = normalized.Planned;
        Interruption = normalized.Interruption;
        Origin = normalized.Origin;
        Result = normalized.Result;
    }
    /// <summary>Obtém os campos atuais para edição parcial.</summary>
    public WorkDetails GetDetails() => new(Description, Project, Epic, Activity, Type, Planned, Interruption, Origin, Result);
    private static DateTimeOffset Truncate(DateTimeOffset value) => new(value.UtcTicks - value.UtcTicks % TimeSpan.TicksPerSecond, TimeSpan.Zero);
    #endregion
}
