using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
namespace KnaulCorp.PersonalTools.Worklog.Application.Reports;
/// <summary>Projeta intervalos em trechos diários sem arredondar os totais.</summary>
public sealed class ReportBuilder(WorklogTime time)
{
    #region Methods
    /// <summary>Recorta os intervalos no período e nas fronteiras locais dos dias.</summary>
    public WorkReportDto Build(IReadOnlyList<WorkEntryDto> entries, DateOnly from, DateOnly to)
    {
        if (to < from) throw new WorklogException("O período está invertido.");
        DateTimeOffset lower = time.GetDayStart(from);
        DateTimeOffset upper = time.GetDayStart(to.AddDays(1));
        WorkEntryDto[] included = entries.Where(entry => entry.StartedAt < upper && (entry.EndedAt is null || entry.EndedAt > lower)).ToArray();
        var segments = new List<ReportSegmentDto>();
        foreach (WorkEntryDto entry in included.Where(entry => entry.EndedAt.HasValue))
        {
            DateTimeOffset start = entry.StartedAt > lower ? entry.StartedAt : lower;
            DateTimeOffset end = entry.EndedAt!.Value < upper ? entry.EndedAt.Value : upper;
            while (start < end)
            {
                DateOnly date = DateOnly.FromDateTime(time.ToLocal(start).DateTime);
                DateTimeOffset midnight = time.GetDayStart(date.AddDays(1));
                DateTimeOffset segmentEnd = midnight < end ? midnight : end;
                segments.Add(new(entry, date, time.ToLocal(start), time.ToLocal(segmentEnd)));
                start = segmentEnd;
            }
        }
        return new(from, to, time.Zone.Id, included.Count(entry => entry.EndedAt.HasValue), included.Count(entry => entry.EndedAt is null), segments);
    }
    #endregion
}
