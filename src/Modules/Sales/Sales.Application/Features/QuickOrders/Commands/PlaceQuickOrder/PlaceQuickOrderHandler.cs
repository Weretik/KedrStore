using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Application.Results;
using Sales.Application.Abstractions.QuickOrders;
using Sales.Application.Features.QuickOrders.Messages;

namespace Sales.Application.Features.QuickOrders.Commands.PlaceQuickOrder;

/// <summary>
/// Stores the order as an outbox message, then tries to send it once (platform spec 006, R-003). The customer gets
/// the order id even when Telegram is down: the dispatcher sends the stored message later. With an idempotency key the
/// key is the order id: a repeat of the same order (a second press, a retry after a lost answer, or a copy that loses
/// the race) answers with the stored order and sends nothing again; the same key with other data is a conflict.
/// </summary>
public sealed class PlaceQuickOrderHandler(IQuickOrderStore store, IOutboxDispatcher outbox, TimeProvider clock)
    : ICommandHandler<PlaceQuickOrderCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(PlaceQuickOrderCommand command, CancellationToken cancellationToken)
    {
        if (command.IdempotencyKey is { } key && await store.FindAsync(key, cancellationToken) is { } stored)
            return Repeat(stored, command);

        var order = new QuickOrderPlaced(
            command.IdempotencyKey ?? Guid.CreateVersion7(), command.FirstName, command.Phone, command.Lines,
            clock.GetUtcNow());

        if (!await store.TrySaveAsync(order, cancellationToken))
        {
            // Another copy of the same request was stored between the check and the save: answer as it did.
            var winner = await store.FindAsync(order.OrderId, CancellationToken.None)
                ?? throw new InvalidOperationException($"Quick order {order.OrderId} was neither saved nor found.");
            return Repeat(winner, command);
        }

        // The order is committed; a cancelled request must not turn it into an error answer.
        await outbox.DispatchAsync([order.OrderId], CancellationToken.None);

        return Result.Success(order.OrderId);
    }

    private static Result<Guid> Repeat(QuickOrderPlaced stored, PlaceQuickOrderCommand command) =>
        stored.FirstName == command.FirstName && stored.Phone == command.Phone && stored.Lines.SequenceEqual(command.Lines)
            ? Result.Success(stored.OrderId)
            : QuickOrderErrors.IdempotencyConflict().ToResult<Guid>();
}
