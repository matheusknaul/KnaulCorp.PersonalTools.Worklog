using LiteBus.Commands.Abstractions;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Domain.Entities;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
using KnaulCorp.PersonalTools.Worklog.Domain.Interfaces;
namespace KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands.EntryStart;
/// <summary>Orquestra EntryStart com validação de domínio.</summary>
public sealed class EntryStartCommandHandler(IWorkEntryRepository repository, TimeProvider clock, OperationRunner<EntryStartCommandHandler> operations) : ICommandHandler<EntryStartCommand, Guid>
{
    #region Methods
    /// <summary>Executa a alteração solicitada.</summary>
    public Task<Guid> HandleAsync(EntryStartCommand command, CancellationToken cancellationToken) => operations.RunAsync(nameof(EntryStartCommand), async () =>
    {
        System.ComponentModel.DataAnnotations.Validator.ValidateObject(command, new(command), true);
        WorkEntry? active = await repository.GetActiveAsync(cancellationToken);
        if (active is not null) throw new WorklogException($"Já existe uma atividade aberta ({active.Id}). Use worklog stop.");
        WorkEntry entry = WorkEntry.Create(command.Start, null, command.Details, clock.GetUtcNow());
        await repository.AddAsync(entry, cancellationToken);
        return entry.Id;
    });
    #endregion
}
