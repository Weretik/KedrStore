using Scheduling.Domain.Tasks;

namespace Scheduling.Domain.Runs;

/// <summary>One run of a task: claimed by the tick, started and finished by the job (platform/007 R-04, R-06).</summary>
public sealed class ScheduledTaskRun
{
    private ScheduledTaskRun()
    {
        TaskKey = string.Empty;
    }

    internal ScheduledTaskRun(Guid id, string taskKey, RunTrigger trigger, string? requestedBy, DateTimeOffset claimedAt)
    {
        Id = id;
        TaskKey = taskKey;
        Trigger = trigger;
        RequestedBy = requestedBy;
        ClaimedAt = claimedAt;
        Outcome = RunOutcome.Running;
    }

    public Guid Id { get; private set; }
    public string TaskKey { get; private set; }
    public RunTrigger Trigger { get; private set; }
    public string? RequestedBy { get; private set; }
    public DateTimeOffset ClaimedAt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? FinishedAt { get; private set; }
    public RunOutcome Outcome { get; private set; }
    public string? Message { get; private set; }

    internal void Start(DateTimeOffset now) => StartedAt ??= now;

    internal void Finish(bool succeeded, string? message, DateTimeOffset now)
    {
        Outcome = succeeded ? RunOutcome.Succeeded : RunOutcome.Failed;
        Message = message is null ? null : ScheduledTaskErrors.Shorten(message);
        FinishedAt = now;
    }
}
