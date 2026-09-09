using Sales.Application.Features.Orders.Create.DTOs;

namespace Sales.Application.Features.Orders.Create;

public sealed record CreateOrderCommand(
    CreateOrderRequest Request,
    Guid IdempotencyKey) : ICommand<Result<CreateOrderResult>>;
