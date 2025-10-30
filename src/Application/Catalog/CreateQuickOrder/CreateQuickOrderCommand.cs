using Application.Catalog.CreateQuickOrder.DTOs;

namespace Application.Catalog.CreateQuickOrder;

public sealed record CreateQuickOrderCommand(QuickOrderRequest Request) : ICommand<Result>;
