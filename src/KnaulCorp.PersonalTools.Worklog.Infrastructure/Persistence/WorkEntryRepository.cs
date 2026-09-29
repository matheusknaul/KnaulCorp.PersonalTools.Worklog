using Microsoft.EntityFrameworkCore;
using Npgsql;
using KnaulCorp.PersonalTools.Worklog.Domain.Entities;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
using KnaulCorp.PersonalTools.Worklog.Domain.Interfaces;
namespace KnaulCorp.PersonalTools.Worklog.Infrastructure.Persistence;
/// <summary>Persiste intervalos e traduz conflitos de banco em erros de negócio.</summary>
public sealed class WorkEntryRepository(WorklogDbContext context) : IWorkEntryRepository
{
    #region Methods
    /// <inheritdoc />
    public Task<WorkEntry?> GetAsync(Guid id, CancellationToken cancellationToken) => context.Entries.SingleOrDefaultAsync(entry => entry.Id == id, cancellationToken);
    /// <inheritdoc />
    public Task<WorkEntry?> GetActiveAsync(CancellationToken cancellationToken) => context.Entries.SingleOrDefaultAsync(entry => entry.EndedAt == null, cancellationToken);
    /// <inheritdoc />
    public async Task AddAsync(WorkEntry entry, CancellationToken cancellationToken)
    {
        await context.Entries.AddAsync(entry, cancellationToken);
        await SaveAsync(cancellationToken);
    }
    /// <inheritdoc />
    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        try { await context.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException)
        {
            throw new WorklogException("O registro foi alterado por outro processo. Consulte novamente antes de editar.");
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.ExclusionViolation or PostgresErrorCodes.UniqueViolation })
        {
            throw new WorklogException("O intervalo conflita com outro registro. Consulte worklog list ou worklog status e ajuste os horários.");
        }
    }
    #endregion
}
