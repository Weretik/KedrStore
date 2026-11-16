namespace Sales.Application.Abstractions.Orders;

/// <summary>How far the outbox got with an order's 1C message (platform spec 006, R-012).</summary>
public sealed record OneCMessageState(
    int Attempts,
    DateTimeOffset NextAttemptAtUtc,
    DateTimeOffset? LastAttemptAtUtc,
    DateTimeOffset? ProcessedAtUtc,
    DateTimeOffset? FailedAtUtc,
    string? LastError);
