using Sales.Domain.Orders.Enums;

namespace Sales.Application.Contracts.Orders;

public sealed record OrderSyncRetryStoreResult(
    OrderSyncRetryStoreOutcome Outcome,
    long OrderId,
    string? OrderNumber,
    OneCOrderSyncStatus? PreviousStatus);
