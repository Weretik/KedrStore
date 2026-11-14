namespace Sales.Api.Features.Orders;

public sealed record GetAdminOrderLineResponse(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal Amount);
