using Sales.Application.Features.QuickOrders.Messages;

namespace Sales.Application.Abstractions.QuickOrders;

public interface IQuickOrderStore
{
    /// <summary>Saves the order as an outbox message in one transaction.</summary>
    /// <returns>The outbox message id, for the attempt right after the commit.</returns>
    Task<Guid> SaveAsync(QuickOrderPlaced order, CancellationToken cancellationToken);
}
