using Sales.Application.Features.QuickOrders.Messages;
using Sales.Domain.QuickOrders;

namespace Sales.Application.Abstractions.QuickOrders;

/// <summary>Site orders and their Telegram outbox messages, written together in one transaction (spec sales/009).</summary>
public interface IQuickOrderStore
{
    /// <summary>The stored order with this id, tracked for a change.</summary>
    Task<QuickOrder?> FindAsync(QuickOrderId id, CancellationToken cancellationToken);

    /// <summary>
    /// Stores a new order with its Telegram message (under the order id) and its 1C message (under
    /// <see cref="QuickOrder.OneCOrderMessageId"/>) in one save.
    /// </summary>
    /// <returns>False when an order with this id is already stored (a repeat that lost the race).</returns>
    Task<bool> TryAddAsync(
        QuickOrder order, QuickOrderPlaced message, QuickOrderToOneC oneCMessage, CancellationToken cancellationToken);

    /// <summary>Saves the resend transition of a tracked order with a new outbox message in one save.</summary>
    /// <returns>False when the order changed meanwhile (another resend or a delivery).</returns>
    Task<bool> TrySaveResendAsync(QuickOrder order, QuickOrderPlaced message, CancellationToken cancellationToken);
}
