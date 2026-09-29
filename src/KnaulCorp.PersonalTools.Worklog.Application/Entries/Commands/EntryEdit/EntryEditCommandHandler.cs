using LiteBus.Commands.Abstractions;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Domain.Entities;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
using KnaulCorp.PersonalTools.Worklog.Domain.Interfaces;
namespace KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands.EntryEdit;
/// <summary>Orquestra EntryEdit com validação de domínio.</summary>
public sealed class EntryEditCommandHandler(IWorkEntryRepository repository, TimeProvider clock, OperationRunner<EntryEditCommandHandler> operations) : ICommandHandler<EntryEditCommand, Guid>
{
    #region Methods
    /// <summary>Executa a alteração solicitada.</summary>
    public Task<Guid> HandleAsync(EntryEditCommand command, CancellationToken cancellationToken) => operations.RunAsync(nameof(EntryEditCommand), async () =>
    {
        System.ComponentModel.DataAnnotations.Validator.ValidateObject(command, new(command), true);
        if (command.Start is null && command.End is null && command.Patch.Supplied.Count == 0 && command.Patch.Cleared.Count == 0) throw new WorklogException("Informe ao menos uma alteração.");
        WorkEntry entry = await repository.GetAsync(command.Id, cancellationToken) ?? throw new WorklogException("Registro não encontrado.");
        if (entry.Version != command.ExpectedVersion) throw new WorklogException("O registro foi alterado por outro processo. Consulte novamente.");
        entry.Update(command.Start ?? entry.StartedAt, command.End ?? entry.EndedAt, command.Patch.Apply(entry.GetDetails()), clock.GetUtcNow());
        await repository.SaveAsync(cancellationToken);
        return entry.Id;
    });
    #endregion
}
