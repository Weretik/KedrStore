namespace Application.Catalog.Commands.CreateQuickOrder;

public sealed record CreateQuickOrderCommand(
    string Name,
    string Phone)
    : ICommand<Result>;
