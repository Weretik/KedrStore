namespace Catalog.Application.Features.Orders.Create.DTOs;

public sealed record CreateOrderRequest(
    string FirstName,
    string Phone,
    IReadOnlyList<CreateOrderLineRequest> Lines);

