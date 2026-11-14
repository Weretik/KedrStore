using Sales.Domain.Orders;

namespace Sales.Application.Features.Orders.Queries.GetOrderSyncStatus;

public sealed record GetOrderSyncStatusResult(
    long OrderId,
    string OrderNumber,
    OneCOrderSyncStatus SyncStatus,
    string? OneCDocumentNumber,
    DateTimeOffset? AcceptedAtUtc);
