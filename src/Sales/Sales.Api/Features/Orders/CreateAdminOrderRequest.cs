namespace Sales.Api.Features.Orders;

public sealed record CreateAdminOrderRequest(
    string CounterpartyId,
    string? Comment,
    IReadOnlyList<CreateAdminOrderLineRequest> Lines);
