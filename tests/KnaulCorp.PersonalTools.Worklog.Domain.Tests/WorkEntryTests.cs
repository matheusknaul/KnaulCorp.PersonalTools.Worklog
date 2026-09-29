using KnaulCorp.PersonalTools.Worklog.Domain.Entities;
using KnaulCorp.PersonalTools.Worklog.Domain.ValueObjects;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
namespace KnaulCorp.PersonalTools.Worklog.Domain.Tests;
/// <summary>Valida invariantes do registro.</summary>
public sealed class WorkEntryTests
{
    #region Fields
    private static readonly DateTimeOffset Start = DateTimeOffset.Parse("2026-09-28T12:12:00-03:00");
    private static readonly DateTimeOffset Now = Start.AddDays(2);
    #endregion
    #region Methods
    [Fact]
    public void Create_WhenValid_ShouldNormalizeTextAndUtc()
    {
        // Arrange
        var details = new WorkDetails(" Revisão ", " ERP ", Activity: "0036347", Interruption: " ");
        // Act
        WorkEntry entry = WorkEntry.Create(Start, Start.AddMinutes(23), details, Now);
        // Assert
        entry.Description.Should().Be("Revisão");
        entry.Project.Should().Be("ERP");
        entry.Activity.Should().Be("0036347");
        entry.Interruption.Should().BeNull();
        entry.StartedAt.Offset.Should().Be(TimeSpan.Zero);
        (entry.EndedAt - entry.StartedAt).Should().Be(TimeSpan.FromMinutes(23));
    }
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WhenEndNotAfterStart_ShouldReject(int minutes)
    {
        // Arrange
        var details = new WorkDetails("Revisão");
        // Act
        Action action = () => WorkEntry.Create(Start, Start.AddMinutes(minutes), details, Now);
        // Assert
        action.Should().Throw<WorklogException>();
    }
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenDescriptionMissing_ShouldReject(string? description)
    {
        // Arrange
        var details = new WorkDetails(description!);
        // Act
        Action action = () => WorkEntry.Create(Start, null, details, Now);
        // Assert
        action.Should().Throw<WorklogException>();
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_WhenInstantIsFuture_ShouldReject(bool futureStart)
    {
        // Arrange
        DateTimeOffset start = futureStart ? Now.AddSeconds(1) : Start;
        DateTimeOffset? end = futureStart ? null : Now.AddSeconds(1);
        // Act
        Action action = () => WorkEntry.Create(start, end, new("Revisão"), Now);
        // Assert
        action.Should().Throw<WorklogException>();
    }
    [Fact]
    public void Update_WhenReopening_ShouldRejectWithoutChangingState()
    {
        // Arrange
        WorkEntry entry = WorkEntry.Create(Start, Start.AddMinutes(23), new("Revisão"), Now);
        Guid version = entry.Version;
        // Act
        Action action = () => entry.Update(Start, null, new("Outro"), Now);
        // Assert
        action.Should().Throw<WorklogException>();
        entry.Version.Should().Be(version);
        entry.Description.Should().Be("Revisão");
    }
    [Fact]
    public void Update_WhenClosing_ShouldChangeVersionAndKeepDetails()
    {
        // Arrange
        WorkEntry entry = WorkEntry.Create(Start, null, new("Revisão", "ERP"), Now);
        Guid version = entry.Version;
        // Act
        entry.Update(Start, Start.AddHours(1), entry.GetDetails() with { Result = "Concluído" }, Now);
        // Assert
        entry.Version.Should().NotBe(version);
        entry.Project.Should().Be("ERP");
        entry.Result.Should().Be("Concluído");
    }
    [Theory]
    [InlineData("description", 4001)]
    [InlineData("project", 201)]
    [InlineData("epic", 201)]
    [InlineData("activity", 101)]
    [InlineData("type", 201)]
    [InlineData("origin", 201)]
    [InlineData("interruption", 4001)]
    [InlineData("result", 4001)]
    public void Normalize_WhenFieldTooLong_ShouldReject(string field, int length)
    {
        // Arrange
        string value = new('a', length);
        WorkDetails details = field switch
        {
            "description" => new(value),
            "project" => new("x", Project: value),
            "epic" => new("x", Epic: value),
            "activity" => new("x", Activity: value),
            "type" => new("x", Type: value),
            "origin" => new("x", Origin: value),
            "interruption" => new("x", Interruption: value),
            _ => new("x", Result: value)
        };
        // Act
        Action action = () => details.Normalize();
        // Assert
        action.Should().Throw<WorklogException>();
    }
    #endregion
}
