namespace KnaulCorp.PersonalTools.Worklog.Application.Common.Interfaces;
/// <summary>Registra operações sem incluir dados pessoais.</summary>
public interface IAppLogger<T>
{
    /// <summary>Registra o início.</summary>
    void LogOperationStart(string operation);
    /// <summary>Registra conclusão e duração.</summary>
    void LogOperationEnd(string operation, TimeSpan duration);
    /// <summary>Registra a categoria de falha sem valores sensíveis.</summary>
    void LogOperationError(string operation, Exception exception);
}
