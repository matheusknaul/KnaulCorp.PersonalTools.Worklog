using Dapper;
using LiteBus.Queries.Abstractions;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Common.Interfaces;
namespace KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
/// <summary>Consulta o histórico diretamente como projeção de leitura.</summary>
public sealed class EntrySearchQueryHandler(ISqlConnectionFactory connections, OperationRunner<EntrySearchQueryHandler> operations) : IQueryHandler<EntrySearchQuery, IReadOnlyList<WorkEntryDto>>
{
    #region Methods
    /// <summary>Aplica filtros combinados sem materializar entidades de domínio.</summary>
    public Task<IReadOnlyList<WorkEntryDto>> HandleAsync(EntrySearchQuery query, CancellationToken cancellationToken) => operations.RunAsync<IReadOnlyList<WorkEntryDto>>(nameof(EntrySearchQuery), async () =>
    {
        await using System.Data.Common.DbConnection connection = await connections.OpenAsync(cancellationToken);
        const string sql = """
            SELECT * FROM worklog.entries
            WHERE (CAST(@From AS timestamptz) IS NULL OR "EndedAt" IS NULL OR "EndedAt" > @From)
              AND (CAST(@To AS timestamptz) IS NULL OR "StartedAt" < @To)
              AND (CAST(@Project AS text) IS NULL OR "Project" = @Project)
              AND (CAST(@Type AS text) IS NULL OR "Type" = @Type)
              AND (CAST(@Activity AS text) IS NULL OR "Activity" = @Activity)
              AND (NOT @ActiveOnly OR "EndedAt" IS NULL)
            ORDER BY "StartedAt", "Id"
            """;
        EntrySearchQuery normalized = query with { From = query.From?.ToUniversalTime(), To = query.To?.ToUniversalTime() };
        IEnumerable<WorkEntryDto> entries = await connection.QueryAsync<WorkEntryDto>(new CommandDefinition(sql, normalized, cancellationToken: cancellationToken));
        return entries.ToList();
    });
    #endregion
}
