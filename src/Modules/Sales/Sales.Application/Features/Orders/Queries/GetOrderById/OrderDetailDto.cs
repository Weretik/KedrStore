using Sales.Domain.Orders;

namespace Sales.Application.Features.Orders.Queries.GetOrderById;

public sealed record OrderDetailDto(
    long OrderId,
    string OrderNumber,
    DateTimeOffset CreatedAtUtc,
    OrderCounterpartyDto Counterparty,
    string? Comment,
    IReadOnlyList<OrderLineDto> Lines,
    decimal TotalAmount,
    OrderSyncDto Sync);

public sealed record OrderCounterpartyDto(
    string CounterpartyId,
    string Name,
    string? Phone);

public sealed record OrderLineDto(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal Amount);

public sealed record OrderSyncDto(
    OneCOrderSyncStatus Status,
    string? OneCDocumentNumber,
    DateTimeOffset? AcceptedAtUtc);
