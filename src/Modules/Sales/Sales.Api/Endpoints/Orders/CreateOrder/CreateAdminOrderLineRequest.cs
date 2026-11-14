namespace Sales.Api.Endpoints.Orders.CreateOrder;

public sealed record CreateAdminOrderLineRequest(
    string ProductId,
    int Quantity,
    decimal Amount);
