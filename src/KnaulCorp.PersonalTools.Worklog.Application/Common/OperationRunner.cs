using System.Diagnostics;
using KnaulCorp.PersonalTools.Worklog.Application.Common.Interfaces;
namespace KnaulCorp.PersonalTools.Worklog.Application.Common;
/// <summary>Executa operações com registro centralizado de duração e falhas.</summary>
public sealed class OperationRunner<T>(IAppLogger<T> logger)
{
    #region Methods
    /// <summary>Executa a operação assíncrona e registra seu resultado.</summary>
    public async Task<TResult> RunAsync<TResult>(string operation, Func<Task<TResult>> action)
    {
        logger.LogOperationStart(operation);
        var watch = Stopwatch.StartNew();
        try
        {
            TResult result = await action();
            logger.LogOperationEnd(operation, watch.Elapsed);
            return result;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            logger.LogOperationError(operation, exception);
            throw;
        }
    }
    #endregion
}
