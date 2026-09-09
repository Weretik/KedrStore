using Sales.Domain.Orders.Enums;

namespace Sales.Application.Features.Orders.Create.DTOs;

public sealed record CreateOrderResult(
    long OrderId,
    string OrderNumber,
    OneCOrderSyncStatus SyncStatus);
