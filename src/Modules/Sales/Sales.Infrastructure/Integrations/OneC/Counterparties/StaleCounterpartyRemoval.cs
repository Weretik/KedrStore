using BuildingBlocks.Infrastructure.Outbox.Persistence;
using Sales.Infrastructure.Customers;
using Sales.Domain.Customers;
using Sales.Infrastructure.Persistence.Customers;
using Sales.Infrastructure.Persistence.OrderDrafts;
using Sales.Infrastructure.Persistence.Orders;
namespace Sales.Infrastructure.Integrations.OneC.Counterparties;

public sealed class StaleCounterpartyRemoval(
    SalesDbContext db,
    OutboxWriter outbox,
    ILogger<StaleCounterpartyRemoval> logger)
{
    internal async Task<int> RemoveAsync(Counterparty[] stale, CancellationToken ct)
    {
        if (stale.Length == 0) return 0;

        var staleIds = stale.Select(item => item.Id).ToHashSet();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var orders = await db.Orders.Where(item => staleIds.Contains(item.CounterpartyId)).ToArrayAsync(ct);
        var orderIds = orders.Select(item => item.Id).ToArray();
        var retryAudits = await db.OrderSyncRetryAudits
            .Where(item => orderIds.Contains(item.OrderId)).ToArrayAsync(ct);
        var idempotency = await db.OrderIdempotencyRecords
            .Where(item => orderIds.Contains(item.OrderId)).ToArrayAsync(ct);

        // These records use Restrict FKs. Remove them before Orders so the
        // database can cascade OrderLines safely.
        db.OrderSyncRetryAudits.RemoveRange(retryAudits);
        db.OrderIdempotencyRecords.RemoveRange(idempotency);
        await db.SaveChangesAsync(ct);

        var drafts = await db.OrderDrafts.Where(item => staleIds.Contains(item.CounterpartyId)).ToArrayAsync(ct);
        var rules = await db.CounterpartyCategoryPriceTypes
            .Where(item => staleIds.Contains(item.CounterpartyId)).ToArrayAsync(ct);
        db.Orders.RemoveRange(orders);
        db.OrderDrafts.RemoveRange(drafts);
        db.CounterpartyCategoryPriceTypes.RemoveRange(rules);
        db.Counterparties.RemoveRange(stale);
        // Platform spec 006, R-014: the Identity users go with the customers, through the outbox.
        foreach (var customer in stale)
            outbox.Add(db, new CounterpartyRemoved(customer.Id.Value, customer.IdentityUserId.Value));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        logger.LogInformation("Removed {CounterpartyCount} stale counterparties, {OrderCount} orders and {DraftCount} drafts locally.",
            stale.Length, orders.Length, drafts.Length);
        return stale.Length;
    }
}
