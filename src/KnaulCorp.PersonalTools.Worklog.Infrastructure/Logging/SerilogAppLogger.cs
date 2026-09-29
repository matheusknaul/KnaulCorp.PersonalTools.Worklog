using Serilog;
using KnaulCorp.PersonalTools.Worklog.Application.Common.Interfaces;
namespace KnaulCorp.PersonalTools.Worklog.Infrastructure.Logging;
/// <summary>Registra apenas metadados técnicos, sem mensagens de banco ou textos pessoais.</summary>
public sealed class SerilogAppLogger<T>(ILogger logger) : IAppLogger<T>
{
    #region Methods
    /// <inheritdoc />
    public void LogOperationStart(string operation) => logger.Information("Início {Service} {Operation}", typeof(T).Name, operation);
    /// <inheritdoc />
    public void LogOperationEnd(string operation, TimeSpan duration) => logger.Information("Término {Service} {Operation} em {DurationMs} ms", typeof(T).Name, operation, duration.TotalMilliseconds);
    /// <inheritdoc />
    public void LogOperationError(string operation, Exception exception) => logger.Error("Falha {Service} {Operation}: {ErrorType}", typeof(T).Name, operation, exception.GetType().Name);
    #endregion
}
