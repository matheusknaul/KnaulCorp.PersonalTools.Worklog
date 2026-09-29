using System.Diagnostics;
using KnaulCorp.PersonalTools.Worklog.Cli.Services;
using KnaulCorp.PersonalTools.Worklog.Cli.Settings;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Tests;
/// <summary>Verifica contratos da linha de comando e execução fora do repositório.</summary>
public sealed class CliTests
{
    #region Methods
    [Theory]
    [InlineData("--help")]
    [InlineData("--version")]
    [InlineData("--no-color", "--help")]
    [InlineData("config", "show")]
    [InlineData("start", "--help")]
    public async Task RunAsync_WhenNoDatabaseAndReadOnlyHelp_ShouldSucceed(params string[] arguments)
    {
        // Arrange
        string[] input = arguments;
        // Act
        var result = await RunAsync(input);
        // Assert
        result.Code.Should().Be(0, result.Error);
        result.Output.Should().NotBeNullOrWhiteSpace();
        result.Output.Should().NotContain("\u001b[");
    }
    [Fact]
    public async Task RunAsync_WhenUnknownOption_ShouldUseStderrAndExitTwo()
    {
        // Arrange
        string[] input = ["start", "--unknown"];
        // Act
        var result = await RunAsync(input);
        // Assert
        result.Code.Should().Be(2); result.Error.Should().NotBeNullOrWhiteSpace();
    }
    [Fact]
    public async Task RunAsync_WhenNoConnection_ShouldExitThreeWithoutSecrets()
    {
        // Arrange
        string[] input = ["doctor"];
        // Act
        var result = await RunAsync(input);
        // Assert
        result.Code.Should().Be(3); result.Error.Should().Contain("ConnectionStrings__Worklog");
    }
    [Fact]
    public void Normalize_WhenFlagLikeDescription_ShouldKeepLiteralValue()
    {
        // Arrange
        string[] arguments = ["--no-color", "start", "--description", "--no-color"];
        // Act
        string[] normalized = CliArguments.Normalize(arguments);
        // Assert
        normalized.Should().Equal("start", "--description", "--no-color", "--no-color");
    }
    [Fact]
    public void GetPatch_WhenClearingAndSettingSameField_ShouldReject()
    {
        // Arrange
        var settings = new EditSettings { Project = "ERP" };
        // Act
        Action action = () => EntryInput.GetPatch(settings, ["project"]);
        // Assert
        action.Should().Throw<WorklogException>();
    }
    [Theory]
    [InlineData("description")]
    [InlineData("unknown")]
    public void GetPatch_WhenInvalidClear_ShouldReject(string field)
    {
        // Arrange
        var settings = new EditSettings();
        // Act
        Action action = () => EntryInput.GetPatch(settings, [field]);
        // Assert
        action.Should().Throw<WorklogException>();
    }
    [Fact]
    public void GetPeriod_WhenTodayAndExplicitRange_ShouldReject()
    {
        // Arrange
        var input = new PeriodInput(new(TimeZoneInfo.Utc, TimeProvider.System));
        var settings = new PeriodSettings { Today = true, From = "2026-09-28", To = "2026-09-28" };
        // Act
        Action action = () => input.GetPeriod(settings);
        // Assert
        action.Should().Throw<WorklogException>();
    }
    private static async Task<(int Code, string Output, string Error)> RunAsync(string[] arguments)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "KnaulCorp.PersonalTools.Worklog.slnx"))) directory = directory.Parent;
        string configuration = AppContext.BaseDirectory.Contains("Release") ? "Release" : "Debug";
        string executable = Path.Combine(directory!.FullName, "src", "KnaulCorp.PersonalTools.Worklog.Cli", "bin", configuration, "net10.0", "KnaulCorp.PersonalTools.Worklog.Cli.dll");
        var start = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true, RedirectStandardInput = true, UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = Path.GetTempPath() };
        start.ArgumentList.Add(executable);
        foreach (string argument in arguments) start.ArgumentList.Add(argument);
        start.Environment["ConnectionStrings__Worklog"] = "";
        start.Environment["DOTNET_ENVIRONMENT"] = "Production";
        using Process process = Process.Start(start)!;
        process.StandardInput.Close();
        Task<string> output = process.StandardOutput.ReadToEndAsync(); Task<string> error = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
        return (process.ExitCode, await output, await error);
    }
    #endregion
}
