namespace Sales.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderRequest(
    string CounterpartyId,
    string? Comment,
    IReadOnlyList<CreateOrderLineRequest> Lines);
