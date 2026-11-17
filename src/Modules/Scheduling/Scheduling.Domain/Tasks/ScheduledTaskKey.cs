namespace Scheduling.Domain.Tasks;

/// <summary>Identity of a scheduled task: its key in the code catalogue.</summary>
public readonly record struct ScheduledTaskKey
{
    public string Value { get; }

    private ScheduledTaskKey(string value) => Value = value;

    public static ScheduledTaskKey Create(string value) => string.IsNullOrWhiteSpace(value)
        ? throw new ArgumentException("A task key is required.", nameof(value))
        : new ScheduledTaskKey(value);

    public static ScheduledTaskKey FromStorage(string value) => new(value);

    public override string ToString() => Value;
}
