namespace Sales.Api.Endpoints.Orders.GetOrderById;

public sealed record GetAdminOrderCounterpartyResponse(
    string CounterpartyId,
    string Name,
    string? Phone);
