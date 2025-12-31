using Catalog.Application.Features.Orders.Commands.CreateQuickOrder.DTOs;

namespace Catalog.Application.Features.Orders.Commands.CreateQuickOrder;

public sealed record CreateQuickOrderCommand(QuickOrderRequest Request) : ICommand<Result>;
