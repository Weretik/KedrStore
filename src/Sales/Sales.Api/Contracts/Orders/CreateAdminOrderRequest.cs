namespace Sales.Api.Contracts.Orders;

public sealed record CreateAdminOrderRequest(
    string CounterpartyId,
    string? Comment,
    IReadOnlyList<CreateAdminOrderLineRequest> Lines);
