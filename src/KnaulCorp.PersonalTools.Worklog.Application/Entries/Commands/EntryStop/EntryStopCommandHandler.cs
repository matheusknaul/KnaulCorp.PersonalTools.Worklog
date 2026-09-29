using LiteBus.Commands.Abstractions;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Domain.Entities;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
using KnaulCorp.PersonalTools.Worklog.Domain.Interfaces;
namespace KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands.EntryStop;
/// <summary>Orquestra EntryStop com validação de domínio.</summary>
public sealed class EntryStopCommandHandler(IWorkEntryRepository repository, TimeProvider clock, OperationRunner<EntryStopCommandHandler> operations) : ICommandHandler<EntryStopCommand, Guid>
{
    #region Methods
    /// <summary>Executa a alteração solicitada.</summary>
    public Task<Guid> HandleAsync(EntryStopCommand command, CancellationToken cancellationToken) => operations.RunAsync(nameof(EntryStopCommand), async () =>
    {
        System.ComponentModel.DataAnnotations.Validator.ValidateObject(command, new(command), true);
        WorkEntry entry = await repository.GetActiveAsync(cancellationToken) ?? throw new WorklogException("Nenhuma atividade aberta. Use worklog start.");
        if (entry.Id != command.ExpectedId || entry.Version != command.ExpectedVersion) throw new WorklogException("A atividade mudou em outro terminal. Consulte worklog status antes de encerrar.");
        entry.Update(entry.StartedAt, command.End, command.Patch.Apply(entry.GetDetails()), clock.GetUtcNow());
        await repository.SaveAsync(cancellationToken);
        return entry.Id;
    });
    #endregion
}
