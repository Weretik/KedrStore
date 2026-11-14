using Scheduling.Domain.Runs;
using Scheduling.Domain.Schedules;
using Scheduling.Domain.Tasks;

namespace UnitTests.Scheduling.Domain;

public sealed class ScheduledTaskTests
{
    private static readonly DateTimeOffset Now = KyivTime.ToInstant(new DateTime(2026, 11, 16, 9, 0, 0));
    private static readonly TaskSchedule NineOnWeekdays = TaskSchedule.DailyAt(TaskSchedule.Weekdays, new TimeOnly(9, 0));

    private static ScheduledTask DueTask() => ScheduledTask.Create("stocks", NineOnWeekdays, enabled: true, Now.AddDays(-1));

    [Fact]
    public void A_due_task_is_claimed_once()
    {
        var task = DueTask();
        Assert.True(task.IsDue(Now));

        var run = task.Claim(Now);

        Assert.Equal(TaskState.Running, task.State);
        Assert.Equal(run.Id, task.CurrentRunId);
        Assert.Equal(RunTrigger.Schedule, run.Trigger);
        Assert.False(task.IsDue(Now));
        Assert.Throws<InvalidOperationException>(() => task.Claim(Now));
    }

    [Fact]
    public void A_disabled_task_waits_until_run_now()
    {
        var task = ScheduledTask.Create("stocks", NineOnWeekdays, enabled: false, Now.AddDays(-1));
        Assert.False(task.IsDue(Now));

        Assert.True(task.RequestRun("director@kedr", Now).IsSuccess);

        Assert.True(task.IsDue(Now));
        var run = task.Claim(Now);
        Assert.Equal(RunTrigger.Manual, run.Trigger);
        Assert.Equal("director@kedr", run.RequestedBy);
        Assert.Null(task.RunRequestedAt);
    }

    [Fact]
    public void Run_now_is_refused_while_running_or_already_requested()
    {
        var task = DueTask();
        Assert.True(task.RequestRun("a", Now).IsSuccess);
        Assert.Equal(ScheduledTaskErrors.AlreadyRequested, task.RequestRun("a", Now).Error);
        task.Claim(Now);
        Assert.Equal(ScheduledTaskErrors.AlreadyRunning, task.RequestRun("a", Now).Error);
    }

    [Fact]
    public void Success_records_the_summary_and_the_next_run()
    {
        var task = DueTask();
        var run = task.Claim(Now);
        task.Start(run, Now);

        Assert.True(task.Succeed(run, "Додано 3 ліди", Now.AddMinutes(2)).IsSuccess);

        Assert.Equal(TaskState.Idle, task.State);
        Assert.Equal("Додано 3 ліди", task.LastSuccessSummary);
        Assert.Equal(Now.AddDays(1), task.NextRunAt);
        Assert.Equal(RunOutcome.Succeeded, run.Outcome);
        Assert.Equal(Now, run.StartedAt);
    }

    [Fact]
    public void Failure_keeps_the_last_success_and_shortens_the_reason()
    {
        var task = DueTask();
        var first = task.Claim(Now);
        task.Succeed(first, "OK", Now);
        var second = task.Claim(Now.AddDays(1));

        task.Fail(second, new string('x', 900), Now.AddDays(1));

        Assert.Equal(Now, task.LastSuccessAt);
        Assert.Equal(ScheduledTaskErrors.MaxMessageLength, task.LastFailureReason!.Length);
        Assert.Equal(RunOutcome.Failed, second.Outcome);
    }

    [Fact]
    public void A_stale_run_changes_nothing()
    {
        var task = DueTask();
        var old = task.Claim(Now);
        task.Fail(old, "killed", Now);
        task.Claim(Now.AddDays(1));

        Assert.Equal(ScheduledTaskErrors.StaleRun, task.Succeed(old, "late", Now.AddDays(1)).Error);
        Assert.Equal(TaskState.Running, task.State);
    }

    [Fact]
    public void A_stuck_run_expires_after_its_limit_since_it_started()
    {
        var task = DueTask();
        var run = task.Claim(Now);
        Assert.True(task.Start(run, Now.AddMinutes(20)).IsSuccess);

        Assert.False(task.ExpireIfStuck(run, 30, Now.AddMinutes(49)));
        Assert.True(task.ExpireIfStuck(run, 30, Now.AddMinutes(51)));

        Assert.Equal(TaskState.Idle, task.State);
        Assert.Equal("Задача не завершилася за 30 хв.", task.LastFailureReason);
        Assert.Equal(RunOutcome.Failed, run.Outcome);
    }

    [Fact]
    public void A_run_that_never_starts_expires_after_the_queue_limit()
    {
        var task = DueTask();
        var run = task.Claim(Now);

        Assert.False(task.ExpireIfStuck(run, 30, Now.AddMinutes(ScheduledTask.QueueLimitMinutes - 1)));
        Assert.True(task.ExpireIfStuck(run, 30, Now.AddMinutes(ScheduledTask.QueueLimitMinutes + 1)));
        Assert.StartsWith("Задача не почалася", task.LastFailureReason);
    }

    [Fact]
    public void Changing_the_schedule_recomputes_the_next_run()
    {
        var task = DueTask();
        var twice = TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(7, 0), new TimeOnly(13, 0));

        Assert.True(task.ChangeSchedule(true, twice, null, "admin", Now).IsSuccess);

        Assert.Equal(Now.AddHours(4), task.NextRunAt);
        Assert.Equal(ScheduledTaskErrors.NextRunInPast, task.ChangeSchedule(true, twice, Now.AddMinutes(-1), "admin", Now).Error);
    }

    [Fact]
    public void Inline_work_records_results_without_a_run()
    {
        var task = ScheduledTask.Create("outbox", TaskSchedule.Interval(5).Value, true, Now);
        task.RecordInline(false, "Telegram недоступний", Now);
        Assert.Equal("Telegram недоступний", task.LastFailureReason);
        Assert.Equal(TaskState.Idle, task.State);
    }
}
