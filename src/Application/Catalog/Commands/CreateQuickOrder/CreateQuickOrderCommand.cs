namespace Application.Catalog.Commands.CreateQuickOrder;

public sealed record CreateQuickOrderCommand(QuickOrderRequest Request) : ICommand<Result>;
