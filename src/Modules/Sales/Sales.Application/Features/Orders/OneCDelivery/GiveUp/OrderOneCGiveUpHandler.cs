using BuildingBlocks.Application.Outbox;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.Orders.OneCDelivery.Messages;

namespace Sales.Application.Features.Orders.OneCDelivery.GiveUp;

/// <summary>
/// The last attempt to send an order to 1C failed: stores the Telegram signal for the managers in the same transaction
/// (platform spec 006, R-009, R-013).
/// </summary>
public sealed class OrderOneCGiveUpHandler(IOrderOneCDeliveryStore store, TimeProvider clock)
    : IOutboxGiveUpHandler<OrderPlaced>
{
    public Task HandleGiveUpAsync(OrderPlaced message, OutboxGiveUp giveUp, CancellationToken cancellationToken)
    {
        store.AddDeliveryFailedSignal(
            new OrderOneCDeliveryFailed(message.OrderId, giveUp.Attempts, giveUp.LastError, clock.GetUtcNow()));
        return Task.CompletedTask;
    }
}
