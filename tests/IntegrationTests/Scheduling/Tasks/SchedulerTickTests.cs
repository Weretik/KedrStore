using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Features.Catalog;
using Scheduling.Application.Features.Tasks.Shared;
using Scheduling.Domain.Runs;
using Scheduling.Domain.Schedules;
using Scheduling.Domain.Tasks;

namespace IntegrationTests.Scheduling.Tasks;

/// <summary>platform/007 SC-01, SC-02, SC-05, SC-06 and the outbox inside the tick.</summary>
public sealed class SchedulerTickTests
{
    private static async Task EnableAsync(SchedulingTestHarness harness, string key, TaskSchedule schedule, DateTimeOffset nextRunAt)
    {
        var task = (await ScheduledTaskRows.LoadAsync(harness.Db, harness.Time.GetUtcNow(), CancellationToken.None))
            .Single(x => x.Task.Key == key).Task;
        Assert.True(task.ChangeSchedule(true, schedule, null, "admin", harness.Time.GetUtcNow()).IsSuccess);
        harness.Db.Entry(task).Property(x => x.NextRunAt).CurrentValue = nextRunAt;
        await harness.Db.SaveChangesAsync();
    }

    [Fact]
    public async Task First_tick_creates_every_catalogue_task_with_its_defaults()
    {
        using var harness = new SchedulingTestHarness();

        await harness.TickAsync();

        var tasks = await harness.Db.Tasks.AsNoTracking().ToListAsync();
        Assert.Equivalent(ScheduledTaskCatalog.Tasks.Select(x => x.Key), tasks.Select(x => x.Key));
        Assert.True(tasks.Where(x => x.Key != ScheduledTaskKeys.Outbox && x.Schedule.Kind != ScheduleKind.Manual).All(x => !x.Enabled));
        Assert.Empty(harness.Launcher.Launches);
    }

    [Fact]
    public async Task Due_tasks_start_in_one_job_execution_and_only_once()
    {
        using var harness = new SchedulingTestHarness();
        var daily = TaskSchedule.DailyAt(TaskSchedule.Weekdays, new TimeOnly(9, 0));
        await EnableAsync(harness, ScheduledTaskKeys.Stocks, daily, harness.Time.GetUtcNow().AddMinutes(-1));
        await EnableAsync(harness, ScheduledTaskKeys.Prices, daily, harness.Time.GetUtcNow().AddMinutes(-1));

        var first = await harness.TickAsync();
        harness.Time.Advance(TimeSpan.FromMinutes(1));
        var second = await harness.TickAsync();

        Assert.Equivalent(new[] { ScheduledTaskKeys.Stocks, ScheduledTaskKeys.Prices }, first.Started);
        Assert.Empty(second.Started);
        Assert.Equal(1, harness.Launcher.Launches.Count());
        Assert.Equivalent(first.Started, harness.Launcher.Launches[0].Select(x => x.Key));
    }

    [Fact]
    public async Task Finished_run_records_success_and_the_next_time()
    {
        using var harness = new SchedulingTestHarness();
        var twiceADay = TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(7, 0), new TimeOnly(13, 0));
        await EnableAsync(harness, ScheduledTaskKeys.Stocks, twiceADay, harness.Time.GetUtcNow());
        await harness.TickAsync();
        var launch = harness.Launcher.Launches.Single().Single();

        harness.Time.Advance(TimeSpan.FromMinutes(3));
        await harness.StartAndFinishAsync(launch, true, "Рядків 10: додано 1.");

        await using var check = harness.NewContext();
        var task = await check.Tasks.SingleAsync(x => x.Key == ScheduledTaskKeys.Stocks);
        var run = await check.Runs.SingleAsync(x => x.Id == launch.RunId);
        Assert.Equal(TaskState.Idle, task.State);
        Assert.Equal("Рядків 10: додано 1.", task.LastSuccessSummary);
        Assert.Equal(KyivTime.ToInstant(new DateTime(2026, 11, 16, 13, 0, 0)), task.NextRunAt);
        Assert.Equal(RunOutcome.Succeeded, run.Outcome);
    }

    [Fact]
    public async Task Failed_run_keeps_the_reason_and_the_last_success()
    {
        using var harness = new SchedulingTestHarness();
        var daily = TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(9, 0));
        await EnableAsync(harness, ScheduledTaskKeys.Stocks, daily, harness.Time.GetUtcNow());
        await harness.TickAsync();

        await harness.StartAndFinishAsync(harness.Launcher.Launches.Single().Single(), false, "Таблицю не прочитано.");

        await using var check = harness.NewContext();
        var task = await check.Tasks.SingleAsync(x => x.Key == ScheduledTaskKeys.Stocks);
        Assert.Equal("Таблицю не прочитано.", task.LastFailureReason);
        Assert.Equal(harness.Time.GetUtcNow(), task.LastFailureAt);
        Assert.Null(task.LastSuccessAt);
        Assert.Equal(TaskState.Idle, task.State);
    }

    [Fact]
    public async Task Job_that_cannot_start_fails_the_tasks_with_the_reason()
    {
        using var harness = new SchedulingTestHarness();
        await EnableAsync(harness, ScheduledTaskKeys.Stocks, TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(9, 0)),
            harness.Time.GetUtcNow());
        harness.Launcher.Error = "немає прав";

        var tick = await harness.TickAsync();

        await using var check = harness.NewContext();
        var task = await check.Tasks.SingleAsync(x => x.Key == ScheduledTaskKeys.Stocks);
        Assert.Empty(tick.Started);
        Assert.Equal("немає прав", tick.LaunchError);
        Assert.Equal(TaskState.Idle, task.State);
        Assert.Equal("Не вдалося запустити задачу: немає прав", task.LastFailureReason);
        Assert.True(task.NextRunAt > harness.Time.GetUtcNow());
    }

    [Fact]
    public async Task Stuck_run_fails_after_its_limit_and_the_task_runs_again()
    {
        using var harness = new SchedulingTestHarness();
        await EnableAsync(harness, ScheduledTaskKeys.Stocks, TaskSchedule.Interval(60).Value, harness.Time.GetUtcNow());
        await harness.TickAsync();
        await using (var job = harness.NewContext())
        {
            var launch = harness.Launcher.Launches.Single().Single();
            Assert.True(await harness.Journal(job).StartAsync(launch.Key, launch.RunId, CancellationToken.None));
        }

        harness.Time.Advance(TimeSpan.FromMinutes(31));
        await harness.TickAsync();

        await using var check = harness.NewContext();
        var task = await check.Tasks.SingleAsync(x => x.Key == ScheduledTaskKeys.Stocks);
        Assert.Equal("Задача не завершилася за 30 хв.", task.LastFailureReason);
        Assert.Equal(TaskState.Idle, task.State);
    }

    [Fact]
    public async Task Outbox_runs_inside_every_tick_and_records_a_failure()
    {
        using var harness = new SchedulingTestHarness();
        await harness.TickAsync();
        harness.Outbox.Failure = "Telegram недоступний";
        await harness.TickAsync();

        await using var check = harness.NewContext();
        var outbox = await check.Tasks.SingleAsync(x => x.Key == ScheduledTaskKeys.Outbox);
        Assert.Equal(2, harness.Outbox.Calls);
        Assert.Equal("Нічого досилати.", outbox.LastSuccessSummary);
        Assert.Equal("Telegram недоступний", outbox.LastFailureReason);
        Assert.Empty(harness.Launcher.Launches);
    }

    [Fact]
    public async Task Two_ticks_cannot_claim_the_same_task()
    {
        using var harness = new SchedulingTestHarness();
        await EnableAsync(harness, ScheduledTaskKeys.Stocks, TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(9, 0)),
            harness.Time.GetUtcNow());
        await using var other = harness.NewContext();
        var seenByOther = await other.Tasks.SingleAsync(x => x.Key == ScheduledTaskKeys.Stocks);

        await harness.TickAsync();
        other.Runs.Add(seenByOther.Claim(harness.Time.GetUtcNow()));

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => other.SaveChangesAsync());
    }

    [Fact]
    public async Task Runs_older_than_ninety_days_are_deleted()
    {
        using var harness = new SchedulingTestHarness();
        await EnableAsync(harness, ScheduledTaskKeys.Stocks, TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(9, 0)),
            harness.Time.GetUtcNow());
        await harness.TickAsync();
        await harness.StartAndFinishAsync(harness.Launcher.Launches.Single().Single(), true, "OK");

        harness.Time.Advance(TimeSpan.FromDays(91));
        harness.Launcher.Error = "вимкнено";
        await harness.TickAsync();

        await using var check = harness.NewContext();
        var runs = await check.Runs.AsNoTracking().ToListAsync();
        Assert.True(runs.All(x => x.ClaimedAt > harness.Time.GetUtcNow().AddDays(-90)));
    }
}
