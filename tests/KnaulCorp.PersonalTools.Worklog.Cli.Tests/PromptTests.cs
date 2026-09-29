using Spectre.Console.Testing;
using KnaulCorp.PersonalTools.Worklog.Cli.Services;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Tests;
/// <summary>Verifica entradas guiadas e renderização em terminais estreitos.</summary>
public sealed class PromptTests
{
    #region Methods
    [Fact]
    public async Task AskAsync_WhenSkippingSuggestions_ShouldReturnNull()
    {
        // Arrange
        using var console = new TestConsole().Interactive();
        console.Input.PushKey(ConsoleKey.Enter);
        // Act
        string? result = await new OptionalInput(console).AskAsync("Projeto", ["ERP"], default);
        // Assert
        result.Should().BeNull();
    }
    [Fact]
    public async Task AskAsync_WhenTypingNewValue_ShouldAllowOutsideSuggestions()
    {
        // Arrange
        using var console = new TestConsole().Interactive();
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushTextWithEnter("Novo [projeto]");
        // Act
        string? result = await new OptionalInput(console).AskAsync("Projeto", ["ERP"], default);
        // Assert
        result.Should().Be("Novo [projeto]");
    }
    [Fact]
    public async Task AskAsync_WhenReusingValue_ShouldKeepLiteralMarkup()
    {
        // Arrange
        using var console = new TestConsole().Interactive();
        console.Input.PushKey(ConsoleKey.DownArrow); console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);
        // Act
        string? result = await new OptionalInput(console).AskAsync("Projeto", ["[ERP]"], default);
        // Assert
        result.Should().Be("[ERP]");
        console.Output.Should().Contain("[ERP]");
    }
    [Fact]
    public void ShowStatus_WhenNarrowAndLiteralMarkup_ShouldRenderWithoutErrors()
    {
        // Arrange
        using var console = new TestConsole().Width(48);
        var time = new WorklogTime(TimeZoneInfo.Utc, TimeProvider.System);
        var ui = new WorklogUi(console, time);
        var entry = new WorkEntryDto { Id = Guid.NewGuid(), StartedAt = DateTimeOffset.UtcNow.AddDays(-1), Description = "Revisão [SAP]", Project = "[ERP]" };
        // Act
        ui.ShowStatus(entry);
        ui.ShowEntries([entry]);
        // Assert
        console.Output.Should().Contain("Revisão [SAP]").And.Contain("[ERP]").And.Contain(entry.Id.ToString());
    }
    #endregion
}

