using Sales.Domain.Orders;

namespace Sales.Application.Features.Orders.Queries.GetOrderList;

public sealed record OrderListItemDto(
    long OrderId,
    string OrderNumber,
    string CounterpartyId,
    string CounterpartyName,
    DateTimeOffset CreatedAtUtc,
    int LineCount,
    decimal TotalAmount,
    OneCOrderSyncStatus SyncStatus,
    string? OneCDocumentNumber);
