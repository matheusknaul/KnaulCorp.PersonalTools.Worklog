namespace KnaulCorp.PersonalTools.Worklog.Application.Reports;
/// <summary>Resultado consolidado do período solicitado.</summary>
public sealed record WorkReportDto(DateOnly From, DateOnly To, string TimeZone, int ClosedCount, int OpenCount, IReadOnlyList<ReportSegmentDto> Segments)
{
    #region Properties
    /// <summary>Total dos trechos incluídos.</summary>
    public TimeSpan Total => TimeSpan.FromTicks(Segments.Sum(segment => segment.Duration.Ticks));
    #endregion
}
