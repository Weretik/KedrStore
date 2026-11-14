namespace BuildingBlocks.Infrastructure.Outbox.Dispatch;

/// <summary>When a failed message is tried again, and when the outbox gives up (platform spec 006, R-004).</summary>
public static class OutboxRetrySchedule
{
    public const int MaximumAttempts = 10;

    public static TimeSpan Retention { get; } = TimeSpan.FromDays(30);

    private static readonly TimeSpan[] Delays =
    [
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromMinutes(30),
        TimeSpan.FromHours(1)
    ];

    /// <summary>The wait after the given number of failed attempts (1 based); hourly after the fifth.</summary>
    public static TimeSpan DelayAfter(int failedAttempts)
        => Delays[Math.Clamp(failedAttempts, 1, Delays.Length) - 1];

    public static bool IsExhausted(int attempts) => attempts >= MaximumAttempts;
}
