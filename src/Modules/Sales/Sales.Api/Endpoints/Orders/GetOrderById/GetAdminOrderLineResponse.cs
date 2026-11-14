namespace Sales.Api.Endpoints.Orders.GetOrderById;

public sealed record GetAdminOrderLineResponse(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal Amount);
