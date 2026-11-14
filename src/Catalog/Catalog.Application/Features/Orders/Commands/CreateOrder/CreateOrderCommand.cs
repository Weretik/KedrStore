namespace Catalog.Application.Features.Orders.Commands.CreateOrder;


public sealed record CreateOrderCommand(CreateOrderRequest Request) : ICommand<Result<QuickOrderResponse>>;
