namespace Catalog.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderLineRequest(
    string ProductId,
    string Title,
    decimal UnitPrice,
    int Quantity
);
