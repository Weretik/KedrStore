namespace Scheduling.Domain.Runs;

/// <summary>Identity of one run of a scheduled task (a version 7 GUID, so runs sort by claim time).</summary>
public readonly record struct ScheduledTaskRunId
{
    public Guid Value { get; }

    private ScheduledTaskRunId(Guid value) => Value = value;

    public static ScheduledTaskRunId Create(Guid value) => value == Guid.Empty
        ? throw new ArgumentException("A run id is required.", nameof(value))
        : new ScheduledTaskRunId(value);

    public static ScheduledTaskRunId FromStorage(Guid value) => new(value);
}
