using BuildingBlocks.Domain.Identifiers;
using Microsoft.Extensions.Logging.Abstractions;
using Sales.Application.Abstractions.Catalog;
using Sales.Application.Abstractions.Customers;
using Sales.Application.Abstractions.Integrations.OneC;
using Sales.Application.Features.OneCSync.Jobs;
using Sales.Domain.Customers;

namespace UnitTests.Sales.Application.Customers;

/// <summary>«Клієнти з 1С», step 2: category price types follow the 1C snapshot through the domain.</summary>
public sealed class CounterpartyPriceRulesSyncTests
{
    [Fact]
    public async Task The_sync_changes_adds_and_removes_rules_and_skips_unknown_references()
    {
        var store = new FakeStore(["cp-1"]);
        store.Rules.Add(Rule(10, 1));
        store.Rules.Add(Rule(20, 1));
        var oneC = new FakeOneC([new("cp-1", 10, 2), new("cp-1", 30, 1), new("cp-1", 99, 1), new("cp-unknown", 10, 1)]);

        var result = await new SyncOneCCounterpartyCategoryPriceTypesJob(oneC, store, new FakeCatalog(),
                NullLogger<SyncOneCCounterpartyCategoryPriceTypesJob>.Instance)
            .RunAsync(CancellationToken.None);

        Assert.Equal((2, 1, 2), (result.ImportedOrUpdated, result.Deleted, result.Skipped));
        Assert.Equal(new[] { (10, 2), (30, 1) },
            store.Rules.Select(rule => (rule.CategoryId.Value, rule.PriceTypeId.Value)).OrderBy(rule => rule.Item1));
        Assert.Equal(1, store.Saves);
    }

    [Fact]
    public void A_rule_refuses_a_non_positive_price_type()
    {
        var rule = Rule(10, 1);

        Assert.True(rule.ChangePriceType(PriceTypeId.FromStorage(0)).IsFailure);
        Assert.Equal(1, rule.PriceTypeId.Value);
    }

    private static CounterpartyCategoryPriceType Rule(int categoryId, int priceTypeId)
        => CounterpartyCategoryPriceType.Create("cp-1", ProductCategoryId.FromStorage(categoryId),
            PriceTypeId.FromStorage(priceTypeId)).Value;

    private sealed class FakeOneC(IReadOnlyList<OneCCounterpartyCategoryPriceTypeDto> rules) : ISalesOneCReadClient
    {
        public Task<IReadOnlyList<OneCCounterpartyDto>> GetCounterpartiesAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<OneCCounterpartyDto>>([]);

        public Task<IReadOnlyList<OneCCounterpartyCategoryPriceTypeDto>> GetCounterpartyCategoryPriceTypesAsync(
            CancellationToken cancellationToken) => Task.FromResult(rules);
    }

    private sealed class FakeCatalog : ICatalogReferenceIds
    {
        public Task<HashSet<int>> ExistingCategoryIdsAsync(IReadOnlyCollection<int> categoryIds, CancellationToken cancellationToken)
            => Task.FromResult(new HashSet<int> { 10, 20, 30 });

        public Task<HashSet<int>> ExistingPriceTypeIdsAsync(IReadOnlyCollection<int> priceTypeIds, CancellationToken cancellationToken)
            => Task.FromResult(new HashSet<int> { 1, 2 });
    }

    private sealed class FakeStore(string[] counterpartyIds) : ICounterpartySyncStore
    {
        public List<CounterpartyCategoryPriceType> Rules { get; } = [];
        public int Saves { get; private set; }

        public Task<IReadOnlyList<Counterparty>> ListAllWithDeletedAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<Counterparty>>([]);

        public Task<HashSet<string>> CounterpartyIdsAsync(CancellationToken cancellationToken)
            => Task.FromResult(counterpartyIds.ToHashSet(StringComparer.Ordinal));

        public void Add(Counterparty counterparty) => throw new NotSupportedException();

        public Task<IReadOnlyList<CounterpartyCategoryPriceType>> ListPriceRulesAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<CounterpartyCategoryPriceType>>(Rules.ToList());

        public void AddPriceRule(CounterpartyCategoryPriceType rule) => Rules.Add(rule);

        public void RemovePriceRules(IReadOnlyCollection<CounterpartyCategoryPriceType> rules)
            => Rules.RemoveAll(rules.Contains);

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            Saves++;
            return Task.CompletedTask;
        }

        public Task<int> RemoveStaleAsync(IReadOnlyCollection<Counterparty> stale, CancellationToken cancellationToken)
            => Task.FromResult(0);
    }
}
