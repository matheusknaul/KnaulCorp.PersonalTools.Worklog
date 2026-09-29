using KnaulCorp.PersonalTools.Worklog.Domain.Entities;
namespace KnaulCorp.PersonalTools.Worklog.Domain.Interfaces;
/// <summary>Persistência de intervalos com proteção de concorrência.</summary>
public interface IWorkEntryRepository
{
    /// <summary>Obtém um intervalo pelo identificador.</summary>
    Task<WorkEntry?> GetAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>Obtém o intervalo aberto.</summary>
    Task<WorkEntry?> GetActiveAsync(CancellationToken cancellationToken);
    /// <summary>Persiste um novo intervalo.</summary>
    Task AddAsync(WorkEntry entry, CancellationToken cancellationToken);
    /// <summary>Persiste alterações verificando a versão original.</summary>
    Task SaveAsync(CancellationToken cancellationToken);
}
