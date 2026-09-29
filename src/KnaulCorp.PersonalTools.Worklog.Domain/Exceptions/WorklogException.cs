namespace KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
/// <summary>Representa uma regra de negócio não atendida.</summary>
public sealed class WorklogException(string message) : Exception(message);
