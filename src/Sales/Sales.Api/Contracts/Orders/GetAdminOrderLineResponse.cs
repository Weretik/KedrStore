namespace Sales.Api.Contracts.Orders;

public sealed record GetAdminOrderLineResponse(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal Amount);
