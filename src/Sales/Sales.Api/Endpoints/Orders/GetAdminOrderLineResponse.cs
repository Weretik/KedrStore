namespace Sales.Api.Endpoints.Orders;

public sealed record GetAdminOrderLineResponse(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal Amount);
