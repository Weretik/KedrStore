namespace Catalog.Application.Features.Orders.Create.DTOs;

public sealed record CreateOrderLineRequest(
    string ProductId,
    string Title,
    decimal UnitPrice,
    int Quantity
);
