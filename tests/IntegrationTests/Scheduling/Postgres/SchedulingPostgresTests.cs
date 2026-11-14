using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Scheduling.Application.Features.Catalog;
using Scheduling.Application.Features.Tasks.Commands.RunScheduledTaskNow;
using Scheduling.Application.Features.Tasks.Commands.UpdateScheduledTaskSchedule;
using Scheduling.Application.Features.Tasks.Queries.GetScheduledTasks;
using Scheduling.Application.Features.Tasks.Shared;
using Scheduling.Application.Features.Tick.Commands.RunSchedulerTick;
using Scheduling.Domain.Schedules;
using IntegrationTests.Scheduling.Tasks;

namespace IntegrationTests.Scheduling.Postgres;

/// <summary>
/// The page, schedule change, run now and tick on real PostgreSQL. Kyiv wall-clock instants carry a +02:00/+03:00
/// offset, and Npgsql refuses to write a non-UTC <see cref="DateTimeOffset"/>, which SQLite never noticed.
/// </summary>
[Collection(SchedulingPostgresCollection.Name)]
public sealed class SchedulingPostgresTests(SchedulingPostgresFixture postgres)
{
    private readonly FakeTimeProvider _time = new(SchedulingTestHarness.MondayNine);

    [SkippableFact]
    public async Task Page_creates_the_task_rows_with_Kyiv_next_runs()
    {
        postgres.SkipIfUnavailable();
        await ResetAsync();

        await using var db = postgres.CreateContext();
        var tasks = await new GetScheduledTasksHandler(db, _time).Handle(new GetScheduledTasksQuery(), CancellationToken.None);

        Assert.True(tasks.IsSuccess);
        Assert.Equal(ScheduledTaskCatalog.Tasks.Count, await postgres.CreateContext().Tasks.CountAsync());
    }

    [SkippableFact]
    public async Task Schedule_change_run_now_and_tick_save_on_PostgreSQL()
    {
        postgres.SkipIfUnavailable();
        await ResetAsync();

        await using (var db = postgres.CreateContext())
        {
            var schedule = new ScheduleInput("daily", Times: ["07:00", "13:00"], Days: [1, 2, 3, 4, 5, 6, 7]);
            var next = new DateTimeOffset(2026, 11, 16, 12, 30, 0, TimeSpan.FromHours(2));
            var updated = await new UpdateScheduledTaskScheduleHandler(db, _time).Handle(
                new UpdateScheduledTaskScheduleCommand(ScheduledTaskKeys.Stocks,
                    new ScheduledTaskScheduleInput(true, schedule, next), "Олена"), CancellationToken.None);
            Assert.True(updated.IsSuccess);
            Assert.Equal(next, updated.Value.NextRunAt);
        }

        await using (var db = postgres.CreateContext())
        {
            var run = await new RunScheduledTaskNowHandler(db, _time)
                .Handle(new RunScheduledTaskNowCommand(ScheduledTaskKeys.Stocks, "Олена"), CancellationToken.None);
            Assert.True(run.IsSuccess);
        }

        await using (var db = postgres.CreateContext())
        {
            var launcher = new FakeLauncher();
            var tick = await new RunSchedulerTickHandler(db, _time, [new FakeOutboxWork()], launcher,
                NullLogger<RunSchedulerTickHandler>.Instance).Handle(new RunSchedulerTickCommand(), CancellationToken.None);
            Assert.True(tick.IsSuccess);
            Assert.Contains(launcher.Launches.SelectMany(x => x), x => x.Key == ScheduledTaskKeys.Stocks);
        }
    }

    private async Task ResetAsync()
    {
        await using var db = postgres.CreateContext();
        await db.Database.ExecuteSqlRawAsync("""DELETE FROM scheduling."ScheduledTaskRuns"; DELETE FROM scheduling."ScheduledTasks";""");
    }
}
