using KnaulCorp.PersonalTools.Worklog.Application.Common.Interfaces;
namespace KnaulCorp.PersonalTools.Worklog.Application.Tests;
/// <summary>Captura o ciclo de operações durante os testes.</summary>
public sealed class TestLogger<T> : IAppLogger<T>
{
    #region Properties
    public int Starts { get; private set; }
    public int Ends { get; private set; }
    public int Errors { get; private set; }
    #endregion
    #region Methods
    public void LogOperationStart(string operation) => Starts++;
    public void LogOperationEnd(string operation, TimeSpan duration) => Ends++;
    public void LogOperationError(string operation, Exception exception) => Errors++;
    #endregion
}
