namespace Sales.Application.Features.Orders.GetById.DTOs;

public sealed record OrderDetailDto(
    long OrderId,
    string OrderNumber,
    DateTimeOffset CreatedAtUtc,
    OrderCounterpartyDto Counterparty,
    string? Comment,
    IReadOnlyList<OrderLineDto> Lines,
    decimal TotalAmount,
    OrderSyncDto Sync);
