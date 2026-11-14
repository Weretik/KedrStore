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
