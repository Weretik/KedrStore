using Sales.Application.Features.QuickOrders.Messages;

namespace Sales.Application.Features.QuickOrders.Commands.PlaceQuickOrder;

/// <summary>A site quick order without an account; the answer is the order id.</summary>
public sealed record PlaceQuickOrderCommand(string FirstName, string Phone, IReadOnlyList<QuickOrderLine> Lines)
    : ICommand<Result<Guid>>;
