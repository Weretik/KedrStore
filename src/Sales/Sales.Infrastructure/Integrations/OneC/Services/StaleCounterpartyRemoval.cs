using Identity.Application.Services;
using Sales.Domain.Customers.Entities;
using Sales.Infrastructure.DataBase.Entities;

namespace Sales.Infrastructure.Integrations.OneC.Services;

public sealed class StaleCounterpartyRemoval(
    SalesDbContext db,
    IImportedCounterpartyIdentityProvisioningService identityProvisioning,
    ILogger<StaleCounterpartyRemoval> logger)
{
    internal async Task<int> RemoveAsync(Counterparty[] stale, CancellationToken ct)
    {
        if (stale.Length == 0) return 0;

        var staleIds = stale.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var orders = await db.Orders.Where(item => staleIds.Contains(item.CounterpartyId)).ToArrayAsync(ct);
        var orderIds = orders.Select(item => item.Id).ToArray();
        var retryAudits = await db.OrderSyncRetryAudits
            .Where(item => orderIds.Contains(item.OrderId)).ToArrayAsync(ct);
        var idempotency = await db.OrderIdempotencyRecords
            .Where(item => orderIds.Contains(item.OrderId)).ToArrayAsync(ct);

        // These records use Restrict FKs. Remove them before Orders so the
        // database can cascade OrderLines and OneCOrderSyncs safely.
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
        var pending = await db.PendingCounterpartyIdentityDeletions
            .Where(item => staleIds.Contains(item.CounterpartyId)).ToArrayAsync(ct);
        db.PendingCounterpartyIdentityDeletions.AddRange(stale
            .Where(item => pending.All(record => record.CounterpartyId != item.Id ||
                record.IdentityUserId != item.IdentityUserId))
            .Select(item =>
            new PendingCounterpartyIdentityDeletion
            {
                CounterpartyId = item.Id,
                IdentityUserId = item.IdentityUserId,
                CreatedAtUtc = DateTimeOffset.UtcNow
            }));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        logger.LogInformation("Removed {CounterpartyCount} stale counterparties, {OrderCount} orders and {DraftCount} drafts locally.",
            stale.Length, orders.Length, drafts.Length);
        return stale.Length;
    }

    internal async Task DrainIdentityDeletionsAsync(CancellationToken ct)
    {
        var pending = await db.PendingCounterpartyIdentityDeletions
            .OrderBy(item => item.CreatedAtUtc).ToArrayAsync(ct);
        foreach (var item in pending)
        {
            if (await db.Counterparties.IgnoreQueryFilters()
                    .AnyAsync(customer => customer.IdentityUserId == item.IdentityUserId, ct))
            {
                db.PendingCounterpartyIdentityDeletions.Remove(item);
                await db.SaveChangesAsync(ct);
                continue;
            }

            try
            {
                var result = await identityProvisioning.DeleteUserAsync(
                    item.IdentityUserId, item.CounterpartyId, ct);
                if (result.IsSuccess)
                {
                    db.PendingCounterpartyIdentityDeletions.Remove(item);
                }
                else
                {
                    item.LastAttemptAtUtc = DateTimeOffset.UtcNow;
                    var reason = string.Join(", ", result.Errors);
                    item.LastError = reason.Length > 500 ? reason[..500] : reason;
                    logger.LogWarning("Identity deletion remains pending for counterparty {CounterpartyId}.", item.CounterpartyId);
                }
                await db.SaveChangesAsync(ct);
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                item.LastAttemptAtUtc = DateTimeOffset.UtcNow;
                item.LastError = error.GetType().Name;
                await db.SaveChangesAsync(ct);
                logger.LogWarning(error, "Identity deletion remains pending for counterparty {CounterpartyId}.", item.CounterpartyId);
            }
        }
    }
}
