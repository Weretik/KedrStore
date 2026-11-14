using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Scheduling.Application.Abstractions;
using Scheduling.Application.Features.Runs;
using Scheduling.Application.Features.Tick.Commands.RunSchedulerTick;
using Scheduling.Domain.Schedules;
using Scheduling.Infrastructure.Persistence;

namespace IntegrationTests.Scheduling.Tasks;

internal sealed class FakeLauncher : ITaskJobLauncher
{
    public List<IReadOnlyList<TaskLaunch>> Launches { get; } = [];
    public string? Error { get; set; }

    public Task<string?> LaunchAsync(IReadOnlyList<TaskLaunch> launches, CancellationToken cancellationToken)
    {
        if (Error is null) Launches.Add(launches);
        return Task.FromResult(Error);
    }
}

internal sealed class FakeOutboxWork : ITickWork
{
    public int Calls { get; private set; }
    public string? Failure { get; set; }
    public string TaskKey => "outbox";

    public Task<string> RunAsync(CancellationToken cancellationToken)
    {
        Calls++;
        return Failure is null ? Task.FromResult("Нічого досилати.") : throw new InvalidOperationException(Failure);
    }
}

/// <summary>
/// SQLite cannot compare or sort DateTimeOffset text; as numbers it can, the way PostgreSQL compares timestamptz.
/// </summary>
internal sealed class SqliteInstantsCustomizer(ModelCustomizerDependencies dependencies) : RelationalModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);
        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(x => x.GetProperties())
                     .Where(x => x.ClrType == typeof(DateTimeOffset) || x.ClrType == typeof(DateTimeOffset?)))
            property.SetValueConverter(new DateTimeOffsetToBinaryConverter());
    }
}

/// <summary>The Scheduling module on SQLite in memory with a fake clock in Kyiv time, a fake job launcher and outbox.</summary>
internal sealed class SchedulingTestHarness : IDisposable
{
    private readonly SqliteConnection _connection;

    /// <summary>Monday 16 November 2026, 09:00 Kyiv.</summary>
    public static readonly DateTimeOffset MondayNine = KyivTime.ToInstant(new DateTime(2026, 11, 16, 9, 0, 0));

    public SchedulingTestHarness(DateTimeOffset? now = null)
    {
        Time = new FakeTimeProvider(now ?? MondayNine);
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        Db = NewContext();
        Db.Database.EnsureCreated();
    }

    public FakeTimeProvider Time { get; }
    public SchedulingDbContext Db { get; }
    public FakeLauncher Launcher { get; } = new();
    public FakeOutboxWork Outbox { get; } = new();

    /// <summary>A second unit of work on the same database, as another tick or the job sees it.</summary>
    public SchedulingDbContext NewContext() =>
        new(new DbContextOptionsBuilder<SchedulingDbContext>().UseSqlite(_connection)
            .ReplaceService<IModelCustomizer, SqliteInstantsCustomizer>().Options);

    public RunSchedulerTickHandler Tick(SchedulingDbContext? db = null) =>
        new(db ?? Db, Time, [Outbox], Launcher, NullLogger<RunSchedulerTickHandler>.Instance);

    /// <summary>One tick in its own unit of work, as each scheduler call has.</summary>
    public async Task<SchedulerTickResponse> TickAsync()
    {
        await using var db = NewContext();
        var result = await Tick(db).Handle(new RunSchedulerTickCommand(), CancellationToken.None);
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    public async Task StartAndFinishAsync(TaskLaunch launch, bool succeeded, string message)
    {
        await using var job = NewContext();
        var journal = Journal(job);
        Assert.True(await journal.StartAsync(launch.Key, launch.RunId, CancellationToken.None));
        await journal.FinishAsync(launch.Key, launch.RunId, succeeded, message, CancellationToken.None);
    }

    public ScheduledTaskRunJournal Journal(SchedulingDbContext db) =>
        new(db, Time, NullLogger<ScheduledTaskRunJournal>.Instance);

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
