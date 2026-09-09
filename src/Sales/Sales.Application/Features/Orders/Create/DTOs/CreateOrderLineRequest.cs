namespace Sales.Application.Features.Orders.Create.DTOs;

public sealed record CreateOrderLineRequest(
    string ProductId,
    int Quantity,
    decimal Amount);
