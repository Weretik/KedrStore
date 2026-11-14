namespace Sales.Api.Endpoints.Orders;

public sealed record CreateAdminOrderLineRequest(
    string ProductId,
    int Quantity,
    decimal Amount);
