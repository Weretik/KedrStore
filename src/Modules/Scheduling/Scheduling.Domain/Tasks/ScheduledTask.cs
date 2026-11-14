using Scheduling.Domain.Runs;
using Scheduling.Domain.Schedules;

namespace Scheduling.Domain.Tasks;

/// <summary>
/// The schedule and last results of one task of the code catalogue (platform/007). The key is the catalogue key; the
/// admin changes only the schedule and the on/off switch. Two ticks never start it twice: a claim moves it from
/// <see cref="TaskState.Idle"/> to <see cref="TaskState.Running"/> and is saved with a concurrency check.
/// </summary>
public sealed class ScheduledTask
{
    private ScheduledTask()
    {
        Key = string.Empty;
        Schedule = TaskSchedule.Manual;
    }

    private ScheduledTask(string key, TaskSchedule schedule, bool enabled, DateTimeOffset now)
    {
        Key = key;
        Schedule = schedule;
        Enabled = enabled;
        NextRunAt = schedule.NextAfter(now);
        UpdatedAt = now;
    }

    public string Key { get; private set; }
    public bool Enabled { get; private set; }
    public TaskSchedule Schedule { get; private set; }
    public DateTimeOffset? NextRunAt { get; private set; }
    public TaskState State { get; private set; }
    public Guid? CurrentRunId { get; private set; }
    public DateTimeOffset? RunningSince { get; private set; }
    public DateTimeOffset? RunRequestedAt { get; private set; }
    public string? RunRequestedBy { get; private set; }
    public DateTimeOffset? LastSuccessAt { get; private set; }
    public string? LastSuccessSummary { get; private set; }
    public DateTimeOffset? LastFailureAt { get; private set; }
    public string? LastFailureReason { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    /// <summary>Concurrency token: every change adds one, so a second tick's claim of the same row fails on save.</summary>
    public int Version { get; private set; }

    public static ScheduledTask Create(string key, TaskSchedule schedule, bool enabled, DateTimeOffset now) =>
        new(key, schedule, enabled, now);

    /// <summary>Requested by «Запустити зараз», or enabled and its time has come; never while it runs.</summary>
    public bool IsDue(DateTimeOffset now) =>
        State == TaskState.Idle && (RunRequestedAt is not null || (Enabled && NextRunAt <= now));

    /// <summary>The tick takes the task (R-04 d); the caller checks <see cref="IsDue"/> first.</summary>
    public ScheduledTaskRun Claim(DateTimeOffset now)
    {
        if (State == TaskState.Running) throw new InvalidOperationException($"Task {Key} is already running.");
        var run = new ScheduledTaskRun(Guid.CreateVersion7(now), Key,
            RunRequestedAt is null ? RunTrigger.Schedule : RunTrigger.Manual, RunRequestedBy, now);
        State = TaskState.Running;
        Version++;
        CurrentRunId = run.Id;
        RunningSince = now;
        RunRequestedAt = null;
        RunRequestedBy = null;
        return run;
    }

    public DomainResult Start(ScheduledTaskRun run, DateTimeOffset now)
    {
        if (!IsCurrent(run)) return ScheduledTaskErrors.StaleRun;
        run.Start(now);
        return DomainResult.Success();
    }

    public DomainResult Succeed(ScheduledTaskRun run, string summary, DateTimeOffset now)
    {
        if (!IsCurrent(run)) return ScheduledTaskErrors.StaleRun;
        run.Finish(true, summary, now);
        RecordSuccess(summary, now);
        Release(now);
        return DomainResult.Success();
    }

    public DomainResult Fail(ScheduledTaskRun run, string reason, DateTimeOffset now)
    {
        if (!IsCurrent(run)) return ScheduledTaskErrors.StaleRun;
        run.Finish(false, reason, now);
        RecordFailure(reason, now);
        Release(now);
        return DomainResult.Success();
    }

    /// <summary>
    /// Tasks claimed together run one after another in one job execution, so a run waits its turn: it may wait up to
    /// this long before it starts (R-04 b). Also the job's own time limit in the deploy workflow.
    /// </summary>
    public const int QueueLimitMinutes = 240;

    /// <summary>
    /// A run that took longer than <paramref name="maxMinutes"/> since it started, or never started within
    /// <see cref="QueueLimitMinutes"/>, is failed and the task freed (R-04 b).
    /// </summary>
    public bool ExpireIfStuck(ScheduledTaskRun? run, int maxMinutes, DateTimeOffset now)
    {
        if (State != TaskState.Running || RunningSince is null) return false;
        string reason;
        if (run?.StartedAt is { } started)
        {
            if (started.AddMinutes(maxMinutes) > now) return false;
            reason = $"Задача не завершилася за {maxMinutes} хв.";
        }
        else
        {
            if (RunningSince.Value.AddMinutes(QueueLimitMinutes) > now) return false;
            reason = $"Задача не почалася за {QueueLimitMinutes} хв.: завдання в Google Cloud до неї не дійшло.";
        }

        run?.Finish(false, reason, now);
        RecordFailure(reason, now);
        Release(now);
        return true;
    }

    /// <summary>Work done inside the tick (the outbox): no run row, only the last results (R-04 a).</summary>
    public void RecordInline(bool succeeded, string message, DateTimeOffset now)
    {
        if (succeeded) RecordSuccess(message, now);
        else RecordFailure(message, now);
        NextRunAt = Schedule.NextAfter(now);
        Version++;
    }

    /// <summary>«Запустити зараз» (R-08): the next tick starts it, even when the schedule is off.</summary>
    public DomainResult RequestRun(string by, DateTimeOffset now)
    {
        if (State == TaskState.Running) return ScheduledTaskErrors.AlreadyRunning;
        if (RunRequestedAt is not null) return ScheduledTaskErrors.AlreadyRequested;
        RunRequestedAt = now;
        RunRequestedBy = by;
        Version++;
        return DomainResult.Success();
    }

    /// <summary>New schedule and on/off; <paramref name="nextRunAt"/> moves the next run once (R-02, R-03).</summary>
    public DomainResult ChangeSchedule(bool enabled, TaskSchedule schedule, DateTimeOffset? nextRunAt, string by,
        DateTimeOffset now)
    {
        if (nextRunAt is { } at && at < now) return ScheduledTaskErrors.NextRunInPast;
        Enabled = enabled;
        Schedule = schedule;
        NextRunAt = nextRunAt ?? schedule.NextAfter(now);
        UpdatedAt = now;
        UpdatedBy = by;
        Version++;
        return DomainResult.Success();
    }

    private bool IsCurrent(ScheduledTaskRun run) =>
        State == TaskState.Running && CurrentRunId == run.Id && run.TaskKey == Key;

    private void RecordSuccess(string summary, DateTimeOffset now)
    {
        LastSuccessAt = now;
        LastSuccessSummary = ScheduledTaskErrors.Shorten(summary);
    }

    private void RecordFailure(string reason, DateTimeOffset now)
    {
        LastFailureAt = now;
        LastFailureReason = ScheduledTaskErrors.Shorten(reason);
    }

    private void Release(DateTimeOffset now)
    {
        State = TaskState.Idle;
        Version++;
        CurrentRunId = null;
        RunningSince = null;
        NextRunAt = Schedule.NextAfter(now);
    }
}
