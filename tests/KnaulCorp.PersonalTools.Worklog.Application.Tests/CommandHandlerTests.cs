using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands.EntryStart;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands.EntryAdd;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands.EntryStop;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Commands.EntryEdit;
using KnaulCorp.PersonalTools.Worklog.Domain.Entities;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
namespace KnaulCorp.PersonalTools.Worklog.Application.Tests;
/// <summary>Valida a orquestração das escritas e conflitos esperados.</summary>
public sealed class CommandHandlerTests
{
    #region Fields
    private readonly MemoryEntryRepository _repository = new();
    private readonly DateTimeOffset _start = DateTimeOffset.UtcNow.AddDays(-2);
    #endregion
    #region Methods
    [Fact]
    public async Task HandleAsync_WhenStarting_ShouldSaveAndLogSuccess()
    {
        // Arrange
        var logger = new TestLogger<EntryStartCommandHandler>();
        var handler = new EntryStartCommandHandler(_repository, TimeProvider.System, new(logger));
        // Act
        Guid id = await handler.HandleAsync(new(new("Revisão"), _start), default);
        // Assert
        _repository.Entries.Should().ContainSingle(entry => entry.Id == id && entry.EndedAt == null);
        logger.Starts.Should().Be(1); logger.Ends.Should().Be(1); logger.Errors.Should().Be(0);
    }
    [Fact]
    public async Task HandleAsync_WhenAlreadyActive_ShouldRejectAndLogError()
    {
        // Arrange
        _repository.Entries.Add(WorkEntry.Create(_start, null, new("Existente"), DateTimeOffset.UtcNow));
        var logger = new TestLogger<EntryStartCommandHandler>();
        var handler = new EntryStartCommandHandler(_repository, TimeProvider.System, new(logger));
        // Act
        Func<Task> action = () => handler.HandleAsync(new(new("Nova"), _start.AddHours(1)), default);
        // Assert
        await action.Should().ThrowAsync<WorklogException>();
        _repository.Entries.Should().ContainSingle(); logger.Errors.Should().Be(1); logger.Ends.Should().Be(0);
    }
    [Theory]
    [InlineData(23, true)]
    [InlineData(-1, false)]
    public async Task HandleAsync_WhenAdding_ShouldValidateAndSave(int minutes, bool valid)
    {
        // Arrange
        var handler = new EntryAddCommandHandler(_repository, TimeProvider.System, new(new TestLogger<EntryAddCommandHandler>()));
        var command = new EntryAddCommand(new("Retroativo"), _start, _start.AddMinutes(minutes));
        // Act
        Func<Task> action = () => handler.HandleAsync(command, default);
        // Assert
        if (valid) { await action.Should().NotThrowAsync(); _repository.Entries.Should().ContainSingle(); }
        else { await action.Should().ThrowAsync<WorklogException>(); _repository.Entries.Should().BeEmpty(); }
    }
    [Theory]
    [InlineData("valid")]
    [InlineData("missing")]
    [InlineData("changed")]
    [InlineData("invalid-time")]
    public async Task HandleAsync_WhenStopping_ShouldRespectObservedActivity(string scenario)
    {
        // Arrange
        WorkEntry entry = WorkEntry.Create(_start, null, new("Revisão"), DateTimeOffset.UtcNow);
        if (scenario != "missing") _repository.Entries.Add(entry);
        var handler = new EntryStopCommandHandler(_repository, TimeProvider.System, new(new TestLogger<EntryStopCommandHandler>()));
        var patch = new EntryPatch(new("", Result: "Pronto"), new HashSet<string> { "result" }, new HashSet<string>());
        var command = new EntryStopCommand(entry.Id, scenario == "changed" ? Guid.NewGuid() : entry.Version, scenario == "invalid-time" ? _start.AddHours(-1) : _start.AddHours(1), patch);
        // Act
        Func<Task> action = () => handler.HandleAsync(command, default);
        // Assert
        if (scenario == "valid") { await action.Should().NotThrowAsync(); entry.Result.Should().Be("Pronto"); _repository.Saves.Should().Be(1); }
        else { await action.Should().ThrowAsync<WorklogException>(); _repository.Saves.Should().Be(0); }
    }
    [Theory]
    [InlineData("valid")]
    [InlineData("missing")]
    [InlineData("changed")]
    [InlineData("empty")]
    [InlineData("invalid-time")]
    public async Task HandleAsync_WhenEditing_ShouldPreserveUnspecifiedFields(string scenario)
    {
        // Arrange
        WorkEntry entry = WorkEntry.Create(_start, _start.AddHours(1), new("Revisão", "ERP", Result: "Antigo"), DateTimeOffset.UtcNow);
        if (scenario != "missing") _repository.Entries.Add(entry);
        var handler = new EntryEditCommandHandler(_repository, TimeProvider.System, new(new TestLogger<EntryEditCommandHandler>()));
        var patch = new EntryPatch(new(""), new HashSet<string>(), scenario == "empty" ? new HashSet<string>() : new HashSet<string> { "result" });
        var command = new EntryEditCommand(entry.Id, scenario == "changed" ? Guid.NewGuid() : entry.Version, scenario == "invalid-time" ? _start.AddHours(2) : null, null, patch);
        // Act
        Func<Task> action = () => handler.HandleAsync(command, default);
        // Assert
        if (scenario == "valid") { await action.Should().NotThrowAsync(); entry.Project.Should().Be("ERP"); entry.Result.Should().BeNull(); }
        else { await action.Should().ThrowAsync<WorklogException>(); _repository.Saves.Should().Be(0); }
    }
    #endregion
}
