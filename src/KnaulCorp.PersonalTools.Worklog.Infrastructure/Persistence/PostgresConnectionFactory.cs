using System.Data.Common;
using Npgsql;
using KnaulCorp.PersonalTools.Worklog.Application.Common.Interfaces;
namespace KnaulCorp.PersonalTools.Worklog.Infrastructure.Persistence;
/// <summary>Fornece conexões PostgreSQL abertas assincronamente.</summary>
public sealed class PostgresConnectionFactory(NpgsqlDataSource source) : ISqlConnectionFactory
{
    #region Methods
    /// <inheritdoc />
    public async Task<DbConnection> OpenAsync(CancellationToken cancellationToken) => await source.OpenConnectionAsync(cancellationToken);
    #endregion
}
