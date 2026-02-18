using Catalog.Application.Features.Orders.Create.DTOs;

namespace Catalog.Application.Features.Orders.Create;


public sealed record CreateOrderCommand(CreateOrderRequest Request) : ICommand<Result<QuickOrderResponse>>;
