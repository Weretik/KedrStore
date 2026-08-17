using Sales.Domain.Orders.Enums;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Domain.Orders.Entities;

public sealed class OneCOrderSync : BaseAuditableEntity<OneCOrderSyncId>, IAggregateRoot, IAuditableEntity
{
    public OrderId OrderId { get; private set; }
    public OneCOrderSyncStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTimeOffset NextAttemptAtUtc { get; private set; }
    public DateTimeOffset? LastAttemptAtUtc { get; private set; }
    public int? LastHttpStatus { get; private set; }
    public string? LastErrorCode { get; private set; }
    public string? LastErrorMessage { get; private set; }
    public string? OneCRequestPayloadHash { get; private set; }
    public string? OneCResponseBody { get; private set; }
    public DateTimeOffset? AcceptedAtUtc { get; private set; }
    public DateTimeOffset? DeadLetterNotifiedAtUtc { get; private set; }

    private OneCOrderSync() { }

    private OneCOrderSync(OrderId orderId, DateTimeOffset now)
    {
        OrderId = orderId;
        Status = OneCOrderSyncStatus.Pending;
        NextAttemptAtUtc = now;
        MarkAsCreated(now);
    }

    public static OneCOrderSync Create(OrderId orderId, DateTimeOffset now) => new(orderId, now);

    public void StartAttempt(DateTimeOffset now, string requestPayloadHash)
    {
        if (Status is OneCOrderSyncStatus.Accepted or OneCOrderSyncStatus.BusinessError or OneCOrderSyncStatus.DeadLetter)
            return;

        AttemptCount++;
        LastAttemptAtUtc = now;
        OneCRequestPayloadHash = Limit(requestPayloadHash, 128);
        Status = OneCOrderSyncStatus.Sent;
        MarkAsUpdated(now);
    }

    public void Accept(DateTimeOffset now, string? responseBody)
    {
        Status = OneCOrderSyncStatus.Accepted;
        AcceptedAtUtc = now;
        OneCResponseBody = Limit(responseBody, 2_000);
        LastErrorCode = null;
        LastErrorMessage = null;
        MarkAsUpdated(now);
    }

    public void MarkBusinessError(DateTimeOffset now, string? message)
    {
        Status = OneCOrderSyncStatus.BusinessError;
        LastErrorMessage = Limit(message, 1_000);
        MarkAsUpdated(now);
    }

    public void RegisterTransportFailure(
        DateTimeOffset now,
        DateTimeOffset nextAttemptAtUtc,
        int maximumAttempts,
        string? errorCode,
        string? errorMessage,
        int? httpStatus = null)
    {
        LastHttpStatus = httpStatus;
        LastErrorCode = Limit(errorCode, 128);
        LastErrorMessage = Limit(errorMessage, 1_000);

        if (AttemptCount >= maximumAttempts)
        {
            Status = OneCOrderSyncStatus.DeadLetter;
            MarkAsUpdated(now);
            return;
        }

        NextAttemptAtUtc = nextAttemptAtUtc;
        Status = OneCOrderSyncStatus.RetryScheduled;
        MarkAsUpdated(now);
    }

    public void Defer(DateTimeOffset now, DateTimeOffset nextAttemptAtUtc)
    {
        NextAttemptAtUtc = nextAttemptAtUtc;
        Status = OneCOrderSyncStatus.RetryScheduled;
        MarkAsUpdated(now);
    }

    public void MoveToDeadLetter(DateTimeOffset now, string? errorMessage)
    {
        Status = OneCOrderSyncStatus.DeadLetter;
        LastErrorMessage = Limit(errorMessage, 1_000);
        MarkAsUpdated(now);
    }

    public void MarkDeadLetterNotified(DateTimeOffset now)
    {
        DeadLetterNotifiedAtUtc = now;
        MarkAsUpdated(now);
    }

    private static string? Limit(string? value, int maximumLength)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, maximumLength)];
}
