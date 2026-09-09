namespace Sales.Api.Contracts.Orders;

public sealed record CreateAdminOrderLineRequest(
    string ProductId,
    int Quantity,
    decimal Amount);
