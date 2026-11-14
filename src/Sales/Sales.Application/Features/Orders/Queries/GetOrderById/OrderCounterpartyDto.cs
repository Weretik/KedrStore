namespace Sales.Application.Features.Orders.Queries.GetOrderById;

public sealed record OrderCounterpartyDto(
    string CounterpartyId,
    string Name,
    string? Phone);
