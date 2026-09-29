using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace KnaulCorp.PersonalTools.Worklog.Infrastructure.Persistence;
/// <summary>Cria o contexto para ferramentas de migrations sem executar a CLI.</summary>
public sealed class WorklogDbContextFactory : IDesignTimeDbContextFactory<WorklogDbContext>
{
    #region Methods
    /// <summary>Usa a conexão fornecida no ambiente, sem expor seu conteúdo.</summary>
    public WorklogDbContext CreateDbContext(string[] args)
    {
        string connection = Environment.GetEnvironmentVariable("ConnectionStrings__Worklog") ?? throw new InvalidOperationException("Configure ConnectionStrings__Worklog para usar as ferramentas de banco.");
        return new(new DbContextOptionsBuilder<WorklogDbContext>().UseNpgsql(connection, provider => provider.MigrationsHistoryTable("__EFMigrationsHistory", "worklog")).Options);
    }
    #endregion
}
