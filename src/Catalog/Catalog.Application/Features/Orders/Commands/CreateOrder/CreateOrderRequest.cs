namespace Catalog.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderRequest(
    string FirstName,
    string Phone,
    IReadOnlyList<CreateOrderLineRequest> Lines);

