namespace Sales.Application.Abstractions.Orders.Sync;

/// <summary>
/// The 1C state of one order with what a manager needs to act on a problem (platform spec 006, R-012): the 1C answer
/// stored on the order wins; otherwise the outbox message tells whether it waits, retries or gave up.
/// </summary>
public sealed record OrderOneCSync(
    OneCOrderSyncStatus Status,
    string? Error,
    int Attempts,
    DateTimeOffset? LastAttemptAtUtc,
    DateTimeOffset? NextAttemptAtUtc)
{
    public static OrderOneCSync From(string? documentNumber, string? rejection, OneCMessageState? message)
    {
        var attempts = message?.Attempts ?? 0;
        var lastAttempt = message?.LastAttemptAtUtc;
        if (documentNumber is not null)
            return new(OneCOrderSyncStatus.Accepted, null, attempts, lastAttempt, null);
        if (rejection is not null)
            return new(OneCOrderSyncStatus.BusinessError, rejection, attempts, lastAttempt, null);
        if (message is null || message.ProcessedAtUtc is not null)
            return new(OneCOrderSyncStatus.Pending, null, attempts, lastAttempt, null);
        if (message.FailedAtUtc is not null)
            return new(OneCOrderSyncStatus.DeadLetter, message.LastError, attempts, lastAttempt, null);
        return attempts > 0
            ? new(OneCOrderSyncStatus.RetryScheduled, message.LastError, attempts, lastAttempt, message.NextAttemptAtUtc)
            : new(OneCOrderSyncStatus.Pending, null, 0, null, message.NextAttemptAtUtc);
    }

    /// <summary>A manager can send the order again (R-013).</summary>
    public bool CanRetry => Status is OneCOrderSyncStatus.BusinessError or OneCOrderSyncStatus.DeadLetter;
}
