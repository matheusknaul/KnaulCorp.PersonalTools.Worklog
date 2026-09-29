using System.Data.Common;
namespace KnaulCorp.PersonalTools.Worklog.Application.Common.Interfaces;
/// <summary>Fornece conexões abertas para consultas.</summary>
public interface ISqlConnectionFactory
{
    /// <summary>Abre uma conexão de leitura.</summary>
    Task<DbConnection> OpenAsync(CancellationToken cancellationToken);
}
