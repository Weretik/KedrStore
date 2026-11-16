namespace BuildingBlocks.Application.Outbox;

/// <summary>How one delivery of an outbox message went (platform spec 006, R-008).</summary>
public sealed record OutboxHandlingResult
{
    private OutboxHandlingResult(OutboxHandlingOutcome outcome, string? error, DateTimeOffset? retryAtUtc)
    {
        Outcome = outcome;
        Error = error;
        RetryAtUtc = retryAtUtc;
    }

    public OutboxHandlingOutcome Outcome { get; }

    /// <summary>Why the attempt failed; set for <see cref="OutboxHandlingOutcome.Failed"/>.</summary>
    public string? Error { get; }

    /// <summary>When to try again; set for <see cref="OutboxHandlingOutcome.RetryAt"/>.</summary>
    public DateTimeOffset? RetryAtUtc { get; }

    /// <summary>The receiver has the message; the outbox marks it processed.</summary>
    public static OutboxHandlingResult Delivered { get; } = new(OutboxHandlingOutcome.Delivered, null, null);

    /// <summary>Not delivered: counts as an attempt and is retried by the schedule until the limit.</summary>
    public static OutboxHandlingResult Failed(string error) =>
        new(OutboxHandlingOutcome.Failed, string.IsNullOrWhiteSpace(error) ? "Delivery failed." : error, null);

    /// <summary>Not tried now (the receiver is closed until then): not an attempt, the message waits until that time.</summary>
    public static OutboxHandlingResult RetryAt(DateTimeOffset retryAtUtc) =>
        new(OutboxHandlingOutcome.RetryAt, null, retryAtUtc);
}

public enum OutboxHandlingOutcome
{
    Delivered,
    Failed,
    RetryAt
}
