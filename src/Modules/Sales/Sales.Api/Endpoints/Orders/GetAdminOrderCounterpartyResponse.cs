namespace Sales.Api.Endpoints.Orders;

public sealed record GetAdminOrderCounterpartyResponse(
    string CounterpartyId,
    string Name,
    string? Phone);
