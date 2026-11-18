using BuildingBlocks.Infrastructure.Outbox.Persistence;
using Sales.Application.Abstractions.Customers;
using Sales.Infrastructure.Customers;

namespace Sales.Infrastructure.Persistence.Customers;

public sealed class CounterpartySyncStore(
    SalesDbContext db,
    OutboxWriter outbox,
    ILogger<CounterpartySyncStore> logger) : ICounterpartySyncStore
{
    public async Task<IReadOnlyList<Counterparty>> ListAllWithDeletedAsync(CancellationToken cancellationToken)
        => await db.Counterparties.IgnoreQueryFilters().ToListAsync(cancellationToken);

    public Task<HashSet<string>> CounterpartyIdsAsync(CancellationToken cancellationToken)
        => db.Counterparties.AsNoTracking().Select(item => item.Id.Value).ToHashSetAsync(StringComparer.Ordinal, cancellationToken);

    public void Add(Counterparty counterparty) => db.Counterparties.Add(counterparty);

    public async Task<IReadOnlyList<CounterpartyCategoryPriceType>> ListPriceRulesAsync(CancellationToken cancellationToken)
        => await db.CounterpartyCategoryPriceTypes.ToListAsync(cancellationToken);

    public void AddPriceRule(CounterpartyCategoryPriceType rule) => db.CounterpartyCategoryPriceTypes.Add(rule);

    public void RemovePriceRules(IReadOnlyCollection<CounterpartyCategoryPriceType> rules)
        => db.CounterpartyCategoryPriceTypes.RemoveRange(rules);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);

    public async Task<int> RemoveStaleAsync(IReadOnlyCollection<Counterparty> stale, CancellationToken ct)
    {
        if (stale.Count == 0) return 0;

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
            stale.Count, orders.Length, drafts.Length);
        return stale.Count;
    }
}
