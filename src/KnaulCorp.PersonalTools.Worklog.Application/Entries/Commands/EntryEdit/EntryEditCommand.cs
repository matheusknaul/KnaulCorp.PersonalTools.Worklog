using System.ComponentModel.DataAnnotations;
using LiteBus.Commands.Abstractions;
using KnaulCorp.PersonalTools.Worklog.Domain.ValueObjects;
namespace KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands.EntryEdit;
/// <summary>Solicitação de EntryEdit.</summary>
public sealed record EntryEditCommand(Guid Id, Guid ExpectedVersion, DateTimeOffset? Start, DateTimeOffset? End, [property: Required] EntryPatch Patch) : ICommand<Guid>;
