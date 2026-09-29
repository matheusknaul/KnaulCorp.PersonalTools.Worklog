using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
using KnaulCorp.PersonalTools.Worklog.Application.Reports;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
namespace KnaulCorp.PersonalTools.Worklog.Application.Tests;
/// <summary>Valida recortes diários e soma sem arredondamento.</summary>
public sealed class ReportBuilderTests
{
    #region Fields
    private readonly WorklogTime _time = new(TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo"), TimeProvider.System);
    #endregion
    #region Methods
    [Fact]
    public void Build_WhenUserExamples_ShouldTotalFiftyThreeMinutes()
    {
        // Arrange
        WorkEntryDto[] entries = [Create("2026-09-28T12:12:00-03:00", 23), Create("2026-09-28T13:50:00-03:00", 30)];
        // Act
        WorkReportDto report = new ReportBuilder(_time).Build(entries, new(2026, 9, 28), new(2026, 9, 28));
        // Assert
        report.Total.Should().Be(TimeSpan.FromMinutes(53));
        report.ClosedCount.Should().Be(2);
    }
    [Fact]
    public void Build_WhenMidnightAndClipped_ShouldSplitWithoutDoubleCounting()
    {
        // Arrange
        WorkEntryDto[] entries = [Create("2026-09-27T23:50:00-03:00", 20)];
        // Act
        WorkReportDto full = new ReportBuilder(_time).Build(entries, new(2026, 9, 27), new(2026, 9, 28));
        WorkReportDto clipped = new ReportBuilder(_time).Build(entries, new(2026, 9, 28), new(2026, 9, 28));
        // Assert
        full.Segments.Should().HaveCount(2).And.OnlyContain(segment => segment.Duration == TimeSpan.FromMinutes(10));
        full.Total.Should().Be(TimeSpan.FromMinutes(20));
        full.ClosedCount.Should().Be(1);
        clipped.Total.Should().Be(TimeSpan.FromMinutes(10));
    }
    [Fact]
    public void Build_WhenOpenAndLongDuration_ShouldExcludeOpenAndKeepHoursAboveTwentyFour()
    {
        // Arrange
        WorkEntryDto[] entries = [Create("2026-09-26T12:00:00-03:00", 1500), new() { StartedAt = DateTimeOffset.Parse("2026-09-28T12:00:00-03:00") }];
        // Act
        WorkReportDto report = new ReportBuilder(_time).Build(entries, new(2026, 9, 26), new(2026, 9, 28));
        // Assert
        report.Total.Should().Be(TimeSpan.FromHours(25));
        report.OpenCount.Should().Be(1);
        WorklogTime.FormatDuration(report.Total).Should().Be("25:00:00");
    }
    [Fact]
    public void Build_WhenDstDay_ShouldUseActualElapsedTime()
    {
        // Arrange
        var time = new WorklogTime(TimeZoneInfo.FindSystemTimeZoneById("America/New_York"), TimeProvider.System);
        WorkEntryDto[] entries = [Create("2026-03-08T00:00:00-05:00", 23 * 60)];
        // Act
        WorkReportDto report = new ReportBuilder(time).Build(entries, new(2026, 3, 8), new(2026, 3, 8));
        // Assert
        report.Total.Should().Be(TimeSpan.FromHours(23));
        report.Segments.Should().ContainSingle();
    }
    [Theory]
    [InlineData("2026-11-01T01:30:00")]
    [InlineData("2026-03-08T02:30:00")]
    public void ParseInstant_WhenAmbiguousOrInvalid_ShouldRequireOffset(string value)
    {
        // Arrange
        var time = new WorklogTime(TimeZoneInfo.FindSystemTimeZoneById("America/New_York"), TimeProvider.System);
        // Act
        Action action = () => time.ParseInstant(value);
        // Assert
        action.Should().Throw<WorklogException>();
    }
    [Fact]
    public void ParseInstant_WhenLocalTime_ShouldMatchExplicitOffset()
    {
        // Arrange
        string value = "2026-09-28T12:12:00";
        // Act
        DateTimeOffset instant = _time.ParseInstant(value);
        // Assert
        instant.Should().Be(DateTimeOffset.Parse(value + "-03:00"));
    }
    private static WorkEntryDto Create(string start, int minutes) => new() { Id = Guid.NewGuid(), StartedAt = DateTimeOffset.Parse(start), EndedAt = DateTimeOffset.Parse(start).AddMinutes(minutes), Description = "Teste" };
    #endregion
}
