using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
using KnaulCorp.PersonalTools.Worklog.Domain.Entities;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
using KnaulCorp.PersonalTools.Worklog.Infrastructure.Logging;
using KnaulCorp.PersonalTools.Worklog.Infrastructure.Persistence;
namespace KnaulCorp.PersonalTools.Worklog.Infrastructure.Tests;
/// <summary>Verifica persistência, concorrência e consultas em bancos descartáveis.</summary>
public sealed class PostgresTests : IAsyncLifetime
{
    #region Fields
    private string? _connection;
    private string? _admin;
    private string? _database;
    #endregion
    #region Methods
    public async Task InitializeAsync()
    {
        _admin = Environment.GetEnvironmentVariable("WORKLOG_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(_admin)) return;
        _database = "worklog_test_" + Guid.NewGuid().ToString("N");
        await using var connection = new NpgsqlConnection(_admin);
        await connection.OpenAsync();
        await using var create = new NpgsqlCommand($"CREATE DATABASE \"{_database}\"", connection);
        await create.ExecuteNonQueryAsync();
        var builder = new NpgsqlConnectionStringBuilder(_admin) { Database = _database, Pooling = false };
        _connection = builder.ConnectionString;
        await using WorklogDbContext context = CreateContext();
        await context.Database.MigrateAsync();
    }
    public async Task DisposeAsync()
    {
        if (_database is null || _admin is null || !_database.StartsWith("worklog_test_", StringComparison.Ordinal)) return;
        await using var connection = new NpgsqlConnection(_admin);
        await connection.OpenAsync();
        await using var drop = new NpgsqlCommand($"DROP DATABASE \"{_database}\" WITH (FORCE)", connection);
        await drop.ExecuteNonQueryAsync();
    }
    [PostgresFact]
    public async Task AddAsync_WhenAdjacent_ShouldPersistAndReload()
    {
        // Arrange
        await using WorklogDbContext context = CreateContext();
        var repository = new WorkEntryRepository(context);
        WorkEntry first = Create(0, 23);
        WorkEntry second = Create(23, 30);
        // Act
        await repository.AddAsync(first, default); await repository.AddAsync(second, default);
        await using WorklogDbContext reader = CreateContext();
        WorkEntry? loaded = await new WorkEntryRepository(reader).GetAsync(first.Id, default);
        // Assert
        loaded.Should().NotBeNull(); loaded!.Activity.Should().Be("0036347");
        (loaded.EndedAt - loaded.StartedAt).Should().Be(TimeSpan.FromMinutes(23));
        (await reader.Entries.CountAsync()).Should().Be(2);
    }
    [PostgresFact]
    public async Task AddAsync_WhenConcurrentOverlap_ShouldAcceptExactlyOne()
    {
        // Arrange
        await using WorklogDbContext left = CreateContext(); await using WorklogDbContext right = CreateContext();
        // Act
        bool[] accepted = await Task.WhenAll(TryAddAsync(left, Create(0, null)), TryAddAsync(right, Create(10, null)));
        // Assert
        accepted.Count(value => value).Should().Be(1);
        await using WorklogDbContext reader = CreateContext();
        (await new WorkEntryRepository(reader).GetActiveAsync(default)).Should().NotBeNull();
        (await reader.Entries.CountAsync()).Should().Be(1);
    }
    [PostgresFact]
    public async Task SaveAsync_WhenConcurrentEdit_ShouldRejectStaleVersion()
    {
        // Arrange
        WorkEntry entry = Create(0, 23);
        await using (WorklogDbContext seed = CreateContext()) await new WorkEntryRepository(seed).AddAsync(entry, default);
        await using WorklogDbContext left = CreateContext(); await using WorklogDbContext right = CreateContext();
        var firstRepository = new WorkEntryRepository(left); var secondRepository = new WorkEntryRepository(right);
        WorkEntry first = (await firstRepository.GetAsync(entry.Id, default))!;
        WorkEntry second = (await secondRepository.GetAsync(entry.Id, default))!;
        first.Update(first.StartedAt, first.EndedAt, first.GetDetails() with { Result = "Primeiro" }, DateTimeOffset.UtcNow);
        second.Update(second.StartedAt, second.EndedAt, second.GetDetails() with { Result = "Segundo" }, DateTimeOffset.UtcNow);
        await firstRepository.SaveAsync(default);
        // Act
        Func<Task> action = () => secondRepository.SaveAsync(default);
        // Assert
        await action.Should().ThrowAsync<WorklogException>();
        await using WorklogDbContext reader = CreateContext();
        (await reader.Entries.SingleAsync()).Result.Should().Be("Primeiro");
    }
    [PostgresFact]
    public async Task SaveAsync_WhenEditOverlaps_ShouldLeaveStoredValuesUnchanged()
    {
        // Arrange
        await using WorklogDbContext context = CreateContext();
        var repository = new WorkEntryRepository(context);
        WorkEntry first = Create(0, 23); WorkEntry second = Create(30, 30);
        await repository.AddAsync(first, default); await repository.AddAsync(second, default);
        first.Update(first.StartedAt, first.StartedAt.AddMinutes(40), first.GetDetails(), DateTimeOffset.UtcNow);
        // Act
        Func<Task> action = () => repository.SaveAsync(default);
        // Assert
        await action.Should().ThrowAsync<WorklogException>();
        await using WorklogDbContext reader = CreateContext();
        ((await reader.Entries.SingleAsync(value => value.Id == first.Id)).EndedAt - first.StartedAt).Should().Be(TimeSpan.FromMinutes(23));
    }
    [PostgresFact]
    public async Task HandleAsync_WhenFiltering_ShouldIntersectDatesAndCombineExactFields()
    {
        // Arrange
        await using WorklogDbContext context = CreateContext();
        var repository = new WorkEntryRepository(context);
        await repository.AddAsync(Create(0, 23), default); await repository.AddAsync(Create(30, 30), default);
        using Serilog.Core.Logger logger = new LoggerConfiguration().CreateLogger();
        await using NpgsqlDataSource source = NpgsqlDataSource.Create(_connection!);
        var handler = new EntrySearchQueryHandler(new PostgresConnectionFactory(source), new(new SerilogAppLogger<EntrySearchQueryHandler>(logger)));
        DateTimeOffset start = DateTimeOffset.Parse("2026-09-18T12:12:00-03:00");
        // Act
        IReadOnlyList<WorkEntryDto> rows = await handler.HandleAsync(new(start.AddMinutes(23), start.AddHours(1), "ERP", "Revisão", "0036347"), default);
        IReadOnlyList<WorkEntryDto> none = await handler.HandleAsync(new(Project: "Outro"), default);
        // Assert
        rows.Should().ContainSingle(); rows[0].StartedAt.Should().Be(start.AddMinutes(30)); none.Should().BeEmpty();
    }
    private WorklogDbContext CreateContext() => new(new DbContextOptionsBuilder<WorklogDbContext>().UseNpgsql(_connection!, provider => provider.MigrationsHistoryTable("__EFMigrationsHistory", "worklog")).Options);
    private static WorkEntry Create(int offset, int? duration)
    {
        DateTimeOffset start = DateTimeOffset.Parse("2026-09-18T12:12:00-03:00").AddMinutes(offset);
        return WorkEntry.Create(start, duration is null ? null : start.AddMinutes(duration.Value), new("Revisão", "ERP", Activity: "0036347", Type: "Revisão"), DateTimeOffset.UtcNow);
    }
    private static async Task<bool> TryAddAsync(WorklogDbContext context, WorkEntry entry)
    {
        try { await new WorkEntryRepository(context).AddAsync(entry, default); return true; }
        catch (WorklogException exception) { exception.Message.Should().Contain("conflita"); return false; }
    }
    #endregion
}
