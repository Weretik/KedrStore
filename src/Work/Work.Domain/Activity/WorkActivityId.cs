namespace Work.Domain.Activity;

/// <summary>Database identity of an activity log entry.</summary>
public readonly record struct WorkActivityId
{
    public long Value { get; }

    private WorkActivityId(long value) => Value = value;

    public static WorkActivityId FromStorage(long value) => new(value);
}
