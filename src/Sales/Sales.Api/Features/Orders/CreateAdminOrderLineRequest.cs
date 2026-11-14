namespace Sales.Api.Features.Orders;

public sealed record CreateAdminOrderLineRequest(
    string ProductId,
    int Quantity,
    decimal Amount);
