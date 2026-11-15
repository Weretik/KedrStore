using BuildingBlocks.Infrastructure.Outbox;
using BuildingBlocks.Infrastructure.Outbox.Persistence;
using BuildingBlocks.Infrastructure.Persistence;
using Sales.Application.Abstractions.QuickOrders;
using Sales.Application.Features.QuickOrders.Messages;

namespace Sales.Infrastructure.QuickOrders;

/// <summary>
/// A quick order is only an outbox message: the managers work it from the Excel file in Telegram. The message id is the
/// order id, so the outbox primary key is the guard against storing the same order twice.
/// </summary>
internal sealed class QuickOrderStore(SalesDbContext dbContext, OutboxWriter outbox) : IQuickOrderStore
{
    public async Task<QuickOrderPlaced?> FindAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var payload = await dbContext.Set<OutboxMessage>().AsNoTracking()
            .Where(x => x.Id == orderId && x.Type == QuickOrderPlaced.TypeName)
            .Select(x => x.Payload)
            .SingleOrDefaultAsync(cancellationToken);
        return payload is null ? null : OutboxMessageType.ReadPayload<QuickOrderPlaced>(payload);
    }

    public async Task<bool> TrySaveAsync(QuickOrderPlaced order, CancellationToken cancellationToken)
    {
        outbox.Add(dbContext, order, order.OrderId);
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
}
