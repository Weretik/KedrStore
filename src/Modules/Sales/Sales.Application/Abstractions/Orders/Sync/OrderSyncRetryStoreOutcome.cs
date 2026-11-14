namespace Sales.Application.Abstractions.Orders.Sync;

public enum OrderSyncRetryStoreOutcome
{
    Scheduled,
    NotFound,
    InvalidStatus,
    ConcurrencyConflict
}
