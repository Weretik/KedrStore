using Sales.Application.Abstractions.DeliveryFailure;
using Sales.Application.Features.Orders.OneCDelivery.Messages;
using Sales.Domain.Orders;

namespace Sales.Application.Abstractions.Orders;

/// <summary>
/// The data the 1C delivery handlers need (platform spec 006, R-011, R-013). It works inside the outbox dispatch: what
/// it loads is tracked and what it adds is saved by the dispatch with the message outcome; it never saves by itself.
/// </summary>
public interface IOrderOneCDeliveryStore
{
    /// <summary>The order with its lines, tracked; null when it was removed with its customer.</summary>
    Task<Order?> FindForDeliveryAsync(long orderId, CancellationToken cancellationToken);

    /// <summary>Stores the Telegram signal of an order that never reached 1C.</summary>
    void AddDeliveryFailedSignal(OrderOneCDeliveryFailed signal);

    /// <summary>The order and customer for the managers' Excel file; null when the order is gone.</summary>
    Task<DeadLetterNotification?> BuildFailureNotificationAsync(
        OrderOneCDeliveryFailed signal,
        CancellationToken cancellationToken);
}
