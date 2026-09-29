using System.ComponentModel.DataAnnotations;
using LiteBus.Commands.Abstractions;
using KnaulCorp.PersonalTools.Worklog.Domain.ValueObjects;
namespace KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands.EntryStart;
/// <summary>Solicitação de EntryStart.</summary>
public sealed record EntryStartCommand([property: Required] WorkDetails Details, DateTimeOffset Start) : ICommand<Guid>;
