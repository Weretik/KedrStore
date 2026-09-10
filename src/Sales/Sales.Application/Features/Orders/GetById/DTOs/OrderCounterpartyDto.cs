namespace Sales.Application.Features.Orders.GetById.DTOs;

public sealed record OrderCounterpartyDto(
    string CounterpartyId,
    string Name,
    string? Phone);
