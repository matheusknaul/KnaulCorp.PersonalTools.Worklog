using KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
namespace KnaulCorp.PersonalTools.Worklog.Application.Reports;
/// <summary>Fração diária de um intervalo encerrado.</summary>
public sealed record ReportSegmentDto(WorkEntryDto Entry, DateOnly Date, DateTimeOffset Start, DateTimeOffset End)
{
    #region Properties
    /// <summary>Duração exata do trecho.</summary>
    public TimeSpan Duration => End - Start;
    #endregion
}
