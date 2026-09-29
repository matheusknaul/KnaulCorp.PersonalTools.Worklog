using LiteBus.Commands.Abstractions;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Domain.Entities;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
using KnaulCorp.PersonalTools.Worklog.Domain.Interfaces;
namespace KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands.EntryAdd;
/// <summary>Orquestra EntryAdd com validação de domínio.</summary>
public sealed class EntryAddCommandHandler(IWorkEntryRepository repository, TimeProvider clock, OperationRunner<EntryAddCommandHandler> operations) : ICommandHandler<EntryAddCommand, Guid>
{
    #region Methods
    /// <summary>Executa a alteração solicitada.</summary>
    public Task<Guid> HandleAsync(EntryAddCommand command, CancellationToken cancellationToken) => operations.RunAsync(nameof(EntryAddCommand), async () =>
    {
        System.ComponentModel.DataAnnotations.Validator.ValidateObject(command, new(command), true);
        WorkEntry entry = WorkEntry.Create(command.Start, command.End, command.Details, clock.GetUtcNow());
        await repository.AddAsync(entry, cancellationToken);
        return entry.Id;
    });
    #endregion
}
