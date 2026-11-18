namespace Sales.Application.Abstractions.Customers;

/// <summary>Customers and their category price types as the 1C sync («Клієнти з 1С») loads and saves them.</summary>
public interface ICounterpartySyncStore
{
    /// <summary>Every counterparty, removed ones included, tracked for changes.</summary>
    Task<IReadOnlyList<Counterparty>> ListAllWithDeletedAsync(CancellationToken cancellationToken);

    Task<HashSet<string>> CounterpartyIdsAsync(CancellationToken cancellationToken);

    void Add(Counterparty counterparty);

    /// <summary>Every category price type, tracked for changes.</summary>
    Task<IReadOnlyList<CounterpartyCategoryPriceType>> ListPriceRulesAsync(CancellationToken cancellationToken);

    void AddPriceRule(CounterpartyCategoryPriceType rule);

    void RemovePriceRules(IReadOnlyCollection<CounterpartyCategoryPriceType> rules);

    Task SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Deletes customers that 1C no longer has, with their orders, drafts and price types, in one transaction,
    /// and queues the removal of their Identity users through the outbox (platform spec 006, R-014).
    /// </summary>
    Task<int> RemoveStaleAsync(IReadOnlyCollection<Counterparty> stale, CancellationToken cancellationToken);
}
