using System.ComponentModel.DataAnnotations;
using LiteBus.Commands.Abstractions;
using KnaulCorp.PersonalTools.Worklog.Domain.ValueObjects;
namespace KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands.EntryAdd;
/// <summary>Solicitação de EntryAdd.</summary>
public sealed record EntryAddCommand([property: Required] WorkDetails Details, DateTimeOffset Start, DateTimeOffset End) : ICommand<Guid>;
