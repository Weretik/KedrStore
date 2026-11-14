using Sales.Domain.Orders;

namespace Sales.Application.Features.Orders.Commands.RetryOrderSync;

public sealed record RetryOrderSyncResult(
    long OrderId,
    string OrderNumber,
    OneCOrderSyncStatus SyncStatus);
