namespace Sales.Application.Contracts.Orders;

public enum OrderSyncRetryStoreOutcome
{
    Scheduled,
    NotFound,
    InvalidStatus,
    ConcurrencyConflict
}
