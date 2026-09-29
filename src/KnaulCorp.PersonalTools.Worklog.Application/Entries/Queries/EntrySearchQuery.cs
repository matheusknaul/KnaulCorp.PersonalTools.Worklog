using LiteBus.Queries.Abstractions;
namespace KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
/// <summary>Pesquisa intervalos por interseção temporal e classificações.</summary>
public sealed record EntrySearchQuery(DateTimeOffset? From = null, DateTimeOffset? To = null, string? Project = null, string? Type = null, string? Activity = null, bool ActiveOnly = false) : IQuery<IReadOnlyList<WorkEntryDto>>;
