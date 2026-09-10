namespace Sales.Application.Features.Orders.GetById.DTOs;

public sealed record OrderLineDto(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal Amount);
