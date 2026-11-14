using Sales.Domain.Orders;

namespace Sales.Application.Features.Orders.Queries.GetOrderById;

public sealed record OrderSyncDto(
    OneCOrderSyncStatus Status,
    string? OneCDocumentNumber,
    DateTimeOffset? AcceptedAtUtc);
