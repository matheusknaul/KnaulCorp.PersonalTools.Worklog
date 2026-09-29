using KnaulCorp.PersonalTools.Worklog.Domain.Entities;
using KnaulCorp.PersonalTools.Worklog.Domain.Interfaces;
namespace KnaulCorp.PersonalTools.Worklog.Application.Tests;
/// <summary>Repositório em memória restrito aos testes de orquestração.</summary>
public sealed class MemoryEntryRepository : IWorkEntryRepository
{
    #region Properties
    public List<WorkEntry> Entries { get; } = [];
    public int Saves { get; private set; }
    #endregion
    #region Methods
    public Task<WorkEntry?> GetAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Entries.SingleOrDefault(entry => entry.Id == id));
    public Task<WorkEntry?> GetActiveAsync(CancellationToken cancellationToken) => Task.FromResult(Entries.SingleOrDefault(entry => entry.EndedAt is null));
    public Task AddAsync(WorkEntry entry, CancellationToken cancellationToken) { Entries.Add(entry); return Task.CompletedTask; }
    public Task SaveAsync(CancellationToken cancellationToken) { Saves++; return Task.CompletedTask; }
    #endregion
}
