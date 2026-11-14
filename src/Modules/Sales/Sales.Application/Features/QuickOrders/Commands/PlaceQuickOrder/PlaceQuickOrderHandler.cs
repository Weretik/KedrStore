using BuildingBlocks.Application.Outbox;
using Sales.Application.Abstractions.QuickOrders;
using Sales.Application.Features.QuickOrders.Messages;

namespace Sales.Application.Features.QuickOrders.Commands.PlaceQuickOrder;

/// <summary>
/// Stores the order as an outbox message, then tries to send it once (platform spec 006, R-003). The customer gets
/// the order id even when Telegram is down: the dispatcher sends the stored message later.
/// </summary>
public sealed class PlaceQuickOrderHandler(IQuickOrderStore store, IOutboxDispatcher outbox, TimeProvider clock)
    : ICommandHandler<PlaceQuickOrderCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(PlaceQuickOrderCommand command, CancellationToken cancellationToken)
    {
        var order = new QuickOrderPlaced(
            Guid.CreateVersion7(), command.FirstName, command.Phone, command.Lines, clock.GetUtcNow());

        var messageId = await store.SaveAsync(order, cancellationToken);

        // The order is committed; a cancelled request must not turn it into an error answer.
        await outbox.DispatchAsync([messageId], CancellationToken.None);

        return Result.Success(order.OrderId);
    }
}
