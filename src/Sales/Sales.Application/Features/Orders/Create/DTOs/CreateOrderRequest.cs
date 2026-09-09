namespace Sales.Application.Features.Orders.Create.DTOs;

public sealed record CreateOrderRequest(
    string CounterpartyId,
    string? Comment,
    IReadOnlyList<CreateOrderLineRequest> Lines);
