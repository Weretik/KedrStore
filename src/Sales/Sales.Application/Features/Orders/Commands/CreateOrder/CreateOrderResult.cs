using Sales.Domain.Orders;

namespace Sales.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderResult(
    long OrderId,
    string OrderNumber,
    OneCOrderSyncStatus SyncStatus);
