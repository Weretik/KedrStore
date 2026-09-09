namespace Sales.Infrastructure.DataBase.Entities;

internal sealed class OrderSyncRetryAudit
{
    public long Id { get; set; }
    public OneCOrderSyncId OneCOrderSyncId { get; set; }
    public OrderId OrderId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public string PreviousStatus { get; set; } = null!;
    public int PreviousAttemptCount { get; set; }
    public string? PreviousErrorCode { get; set; }
    public string? PreviousErrorMessage { get; set; }
    public string Reason { get; set; } = null!;
    public string RequestedBy { get; set; } = null!;
    public DateTimeOffset RequestedAtUtc { get; set; }
}
