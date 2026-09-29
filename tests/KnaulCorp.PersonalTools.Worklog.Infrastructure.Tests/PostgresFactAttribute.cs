namespace KnaulCorp.PersonalTools.Worklog.Infrastructure.Tests;
/// <summary>Executa integração somente com conexão de testes fornecida explicitamente.</summary>
public sealed class PostgresFactAttribute : FactAttribute
{
    #region Constructors
    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WORKLOG_TEST_CONNECTION"))) Skip = "Defina WORKLOG_TEST_CONNECTION para executar integração PostgreSQL.";
    }
    #endregion
}
