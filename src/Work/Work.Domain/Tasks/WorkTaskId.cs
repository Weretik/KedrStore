namespace Work.Domain.Tasks;

/// <summary>Database identity of a staff task.</summary>
public readonly record struct WorkTaskId
{
    public long Value { get; }

    private WorkTaskId(long value) => Value = value;

    public static WorkTaskId Create(long value) => value > 0
        ? new WorkTaskId(value)
        : throw new ArgumentOutOfRangeException(nameof(value));

    public static WorkTaskId FromStorage(long value) => new(value);
}
