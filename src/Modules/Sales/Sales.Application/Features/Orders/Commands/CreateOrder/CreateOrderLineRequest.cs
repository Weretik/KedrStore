namespace Sales.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderLineRequest(
    string ProductId,
    int Quantity,
    decimal Amount);
