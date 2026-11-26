using Sales.Application.Features.QuickOrders.Messages;

namespace Sales.Application.Features.QuickOrders.Commands.PlaceQuickOrder;

/// <summary>
/// A site quick order without an account; the answer is the order id. <paramref name="IdempotencyKey"/> is the
/// <c>Idempotency-Key</c> header the site makes once per order attempt: it becomes the order id, and a repeat with the
/// same key returns that order instead of placing a second one.
/// </summary>
public sealed record PlaceQuickOrderCommand(
    string FirstName,
    string Phone,
    IReadOnlyList<QuickOrderLine> Lines,
    Guid? IdempotencyKey = null,
    string? Comment = null) : ICommand<Result<Guid>>;
