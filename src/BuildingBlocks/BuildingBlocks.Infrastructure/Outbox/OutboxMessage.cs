namespace BuildingBlocks.Infrastructure.Outbox;

/// <summary>One stored follow-up of a module (platform spec 006): what to send, and how far its delivery got.</summary>
public sealed class OutboxMessage
{
    public const int MaximumErrorLength = 1_000;

    private OutboxMessage()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>The registered message type name, stable across class renames.</summary>
    public string Type { get; private set; } = string.Empty;

    /// <summary>The message as JSON.</summary>
    public string Payload { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public int Attempts { get; private set; }

    public DateTimeOffset NextAttemptAtUtc { get; private set; }

    public DateTimeOffset? ProcessedAtUtc { get; private set; }

    /// <summary>Set after the last allowed attempt failed; the message is not tried again.</summary>
    public DateTimeOffset? FailedAtUtc { get; private set; }

    public string? LastError { get; private set; }

    internal static OutboxMessage Create(Guid id, string type, string payload, DateTimeOffset nowUtc) => new()
    {
        Id = id,
        Type = type,
        Payload = payload,
        CreatedAtUtc = nowUtc,
        NextAttemptAtUtc = nowUtc
    };

    internal void MarkProcessed(DateTimeOffset nowUtc)
    {
        Attempts++;
        ProcessedAtUtc = nowUtc;
        LastError = null;
    }

    /// <summary>Records a failed attempt; returns true when it was the last one.</summary>
    internal bool MarkAttemptFailed(DateTimeOffset nowUtc, string error)
    {
        Attempts++;
        LastError = error.Length <= MaximumErrorLength ? error : error[..MaximumErrorLength];

        if (OutboxRetrySchedule.IsExhausted(Attempts))
        {
            FailedAtUtc = nowUtc;
            return true;
        }

        NextAttemptAtUtc = nowUtc + OutboxRetrySchedule.DelayAfter(Attempts);
        return false;
    }
}
