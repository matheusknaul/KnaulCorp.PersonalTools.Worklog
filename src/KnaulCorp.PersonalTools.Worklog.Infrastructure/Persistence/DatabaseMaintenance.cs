using Microsoft.EntityFrameworkCore;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
namespace KnaulCorp.PersonalTools.Worklog.Infrastructure.Persistence;
/// <summary>Executa manutenção explícita e diagnósticos do schema.</summary>
public sealed class DatabaseMaintenance(WorklogDbContext context, OperationRunner<DatabaseMaintenance> operations)
{
    #region Methods
    /// <summary>Aplica migrations somente mediante comando explícito.</summary>
    public Task<int> MigrateAsync(CancellationToken cancellationToken) => operations.RunAsync("Migrate", async () =>
    {
        await context.Database.MigrateAsync(cancellationToken);
        return 0;
    });
    /// <summary>Verifica conectividade e migrations pendentes.</summary>
    public Task<string> DiagnoseAsync(CancellationToken cancellationToken) => operations.RunAsync("Doctor", async () =>
    {
        if (!await context.Database.CanConnectAsync(cancellationToken)) throw new InvalidOperationException("Não foi possível conectar ao PostgreSQL.");
        string[] pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();
        if (pending.Length > 0) throw new InvalidOperationException("Schema desatualizado. Execute worklog db migrate.");
        await context.Entries.AsNoTracking().Take(1).ToListAsync(cancellationToken);
        return "PostgreSQL acessível; schema worklog atualizado.";
    });
    #endregion
}
