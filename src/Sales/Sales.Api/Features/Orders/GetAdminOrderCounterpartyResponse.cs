namespace Sales.Api.Features.Orders;

public sealed record GetAdminOrderCounterpartyResponse(
    string CounterpartyId,
    string Name,
    string? Phone);
