using Sales.Application.Features.Orders.Commands.CreateOrder;

namespace Sales.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    CreateOrderRequest Request,
    Guid IdempotencyKey) : ICommand<Result<CreateOrderResult>>;
