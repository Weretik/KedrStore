using BuildingBlocks.Application.Outbox;
using Sales.Application.Abstractions.DeliveryFailure;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.Orders.OneCDelivery.Messages;

namespace Sales.Application.Features.Orders.OneCDelivery.GiveUp;

/// <summary>Sends the managers an order that never reached 1C, as an Excel file in Telegram (R-013).</summary>
public sealed class OrderOneCDeliveryFailedHandler(
    IOrderOneCDeliveryStore store,
    IOrderDeliveryFailureExporter exporter,
    IDeadLetterNotifier notifier) : IOutboxMessageHandler<OrderOneCDeliveryFailed>
{
    public async Task<OutboxHandlingResult> HandleAsync(
        OrderOneCDeliveryFailed message,
        Guid messageId,
        CancellationToken cancellationToken)
    {
        var notification = await store.BuildFailureNotificationAsync(message, cancellationToken);
        if (notification is null)
            return OutboxHandlingResult.Delivered;

        await notifier.SendAsync(notification, exporter.Build(notification), cancellationToken);
        return OutboxHandlingResult.Delivered;
    }
}
