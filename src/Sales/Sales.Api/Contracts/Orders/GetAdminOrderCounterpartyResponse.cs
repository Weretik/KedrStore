namespace Sales.Api.Contracts.Orders;

public sealed record GetAdminOrderCounterpartyResponse(
    string CounterpartyId,
    string Name,
    string? Phone);
