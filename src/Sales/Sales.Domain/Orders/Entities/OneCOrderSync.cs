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
    public string? OneCDocumentNumber { get; private set; }
    public DateTimeOffset? AcceptedAtUtc { get; private set; }
    public DateTimeOffset? DeadLetterNotifiedAtUtc { get; private set; }
    public int DeadLetterNotificationAttemptCount { get; private set; }
    public DateTimeOffset? DeadLetterNotificationNextAttemptAtUtc { get; private set; }
    public DateTimeOffset? DeadLetterNotificationLastAttemptAtUtc { get; private set; }
    public string? DeadLetterNotificationLastError { get; private set; }
    public DateTimeOffset? DeadLetterNotificationExhaustedAtUtc { get; private set; }

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

    public void Accept(DateTimeOffset now, string documentNumber, string? responseBody = null)
    {
        var normalizedDocumentNumber = Limit(documentNumber, 128)
            ?? throw new ArgumentException("A document number is required for acceptance.", nameof(documentNumber));

        if (OneCDocumentNumber is not null && !string.Equals(OneCDocumentNumber, normalizedDocumentNumber, StringComparison.Ordinal))
            throw new InvalidOperationException("The 1C document number is immutable once assigned.");

        OneCDocumentNumber = normalizedDocumentNumber;
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

    public void ScheduleManualRetry(DateTimeOffset now)
    {
        if (Status is not (OneCOrderSyncStatus.BusinessError or OneCOrderSyncStatus.DeadLetter))
            throw new InvalidOperationException($"Manual retry is not allowed from status {Status}.");

        Status = OneCOrderSyncStatus.RetryScheduled;
        AttemptCount = 0;
        NextAttemptAtUtc = now;
        LastAttemptAtUtc = null;
        LastHttpStatus = null;
        LastErrorCode = null;
        LastErrorMessage = null;
        OneCRequestPayloadHash = null;
        OneCResponseBody = null;
        DeadLetterNotifiedAtUtc = null;
        DeadLetterNotificationAttemptCount = 0;
        DeadLetterNotificationNextAttemptAtUtc = null;
        DeadLetterNotificationLastAttemptAtUtc = null;
        DeadLetterNotificationLastError = null;
        DeadLetterNotificationExhaustedAtUtc = null;
        MarkAsUpdated(now);
    }

    public void MarkDeadLetterNotified(DateTimeOffset now)
    {
        DeadLetterNotifiedAtUtc = now;
        DeadLetterNotificationNextAttemptAtUtc = null;
        DeadLetterNotificationLastError = null;
        MarkAsUpdated(now);
    }

    public bool TryStartDeadLetterNotification(DateTimeOffset now, int maximumAttempts)
    {
        if (Status != OneCOrderSyncStatus.DeadLetter ||
            DeadLetterNotifiedAtUtc.HasValue ||
            DeadLetterNotificationExhaustedAtUtc.HasValue ||
            DeadLetterNotificationAttemptCount >= maximumAttempts ||
            (DeadLetterNotificationNextAttemptAtUtc.HasValue && DeadLetterNotificationNextAttemptAtUtc > now))
            return false;

        DeadLetterNotificationAttemptCount++;
        DeadLetterNotificationLastAttemptAtUtc = now;
        DeadLetterNotificationNextAttemptAtUtc = null;
        MarkAsUpdated(now);
        return true;
    }

    public void RegisterDeadLetterNotificationFailure(
        DateTimeOffset now,
        DateTimeOffset? nextAttemptAtUtc,
        int maximumAttempts,
        string? error)
    {
        DeadLetterNotificationLastError = Limit(error, 1_000);

        if (DeadLetterNotificationAttemptCount >= maximumAttempts)
        {
            DeadLetterNotificationExhaustedAtUtc = now;
            DeadLetterNotificationNextAttemptAtUtc = null;
        }
        else
        {
            DeadLetterNotificationNextAttemptAtUtc = nextAttemptAtUtc;
        }

        MarkAsUpdated(now);
    }

    private static string? Limit(string? value, int maximumLength)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, maximumLength)];
}
