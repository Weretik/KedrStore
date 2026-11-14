using Sales.Domain.Orders;
namespace Sales.Application.Abstractions.Orders.Sync;

public sealed record OrderSyncRetryStoreResult(
    OrderSyncRetryStoreOutcome Outcome,
    long OrderId,
    string? OrderNumber,
    OneCOrderSyncStatus? PreviousStatus);
