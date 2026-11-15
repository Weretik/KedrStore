using Sales.Application.Features.QuickOrders.Messages;

namespace Sales.Application.Abstractions.QuickOrders;

/// <summary>Quick orders are outbox messages whose message id is the order id.</summary>
public interface IQuickOrderStore
{
    /// <summary>The stored order with this id, while its outbox message is kept (outbox retention).</summary>
    Task<QuickOrderPlaced?> FindAsync(Guid orderId, CancellationToken cancellationToken);

    /// <summary>Saves the order as an outbox message under its order id, in one transaction.</summary>
    /// <returns>False when an order with this id is already stored (a repeat that lost the race).</returns>
    Task<bool> TrySaveAsync(QuickOrderPlaced order, CancellationToken cancellationToken);
}
