using BuildingBlocks.Infrastructure.Outbox.Persistence;
using BuildingBlocks.Infrastructure.Persistence;
using Sales.Application.Abstractions.QuickOrders;
using Sales.Application.Features.QuickOrders.Messages;
using Sales.Domain.QuickOrders;

namespace Sales.Infrastructure.Persistence.QuickOrders;

/// <summary>
/// A site order and its Telegram outbox message are written in one save. The outbox message id is the order id, so the
/// order's primary key and the outbox's both reject a second copy of the same order.
/// </summary>
internal sealed class QuickOrderStore(SalesDbContext dbContext, OutboxWriter outbox) : IQuickOrderStore
{
    public Task<QuickOrder?> FindAsync(QuickOrderId id, CancellationToken cancellationToken) =>
        dbContext.Set<QuickOrder>()
            .Include(order => order.Items)
            .SingleOrDefaultAsync(order => order.Id == id, cancellationToken);

    public async Task<bool> TryAddAsync(QuickOrder order, QuickOrderPlaced message, CancellationToken cancellationToken)
    {
        dbContext.Set<QuickOrder>().Add(order);
        outbox.Add(dbContext, message, order.Id.Value);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException error) when (UniqueViolation.Is(error))
        {
            dbContext.ChangeTracker.Clear();
            return false;
        }
    }

    public async Task<bool> TrySaveResendAsync(QuickOrder order, QuickOrderPlaced message, CancellationToken cancellationToken)
    {
        // The outbox reset runs at once; the order's transition (checked by xmin) commits with it or not at all.
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        if (!await outbox.RequeueAsync(dbContext, message, order.Id.Value, cancellationToken))
            return false;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }
}
