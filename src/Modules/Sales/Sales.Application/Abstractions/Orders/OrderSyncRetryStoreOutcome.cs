namespace Sales.Application.Abstractions.Orders;

public enum OrderSyncRetryStoreOutcome
{
    Scheduled,
    NotFound,
    InvalidStatus,
    ConcurrencyConflict
}
