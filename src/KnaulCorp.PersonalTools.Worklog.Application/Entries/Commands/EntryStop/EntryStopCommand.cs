using System.ComponentModel.DataAnnotations;
using LiteBus.Commands.Abstractions;
using KnaulCorp.PersonalTools.Worklog.Domain.ValueObjects;
namespace KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands.EntryStop;
/// <summary>Solicitação de EntryStop.</summary>
public sealed record EntryStopCommand(Guid ExpectedId, Guid ExpectedVersion, DateTimeOffset End, [property: Required] EntryPatch Patch) : ICommand<Guid>;
