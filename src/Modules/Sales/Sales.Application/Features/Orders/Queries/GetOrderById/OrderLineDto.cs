namespace Sales.Application.Features.Orders.Queries.GetOrderById;

public sealed record OrderLineDto(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal Amount);
