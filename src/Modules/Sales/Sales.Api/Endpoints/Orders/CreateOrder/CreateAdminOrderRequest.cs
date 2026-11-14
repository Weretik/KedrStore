namespace Sales.Api.Endpoints.Orders.CreateOrder;

public sealed record CreateAdminOrderRequest(
    string CounterpartyId,
    string? Comment,
    IReadOnlyList<CreateAdminOrderLineRequest> Lines);
