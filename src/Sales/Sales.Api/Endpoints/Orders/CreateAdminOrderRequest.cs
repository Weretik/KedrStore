namespace Sales.Api.Endpoints.Orders;

public sealed record CreateAdminOrderRequest(
    string CounterpartyId,
    string? Comment,
    IReadOnlyList<CreateAdminOrderLineRequest> Lines);
