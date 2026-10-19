using Catalog.Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sales.Application.Contracts.Customers;
using Sales.Domain.Customers.Entities;
using Sales.Infrastructure.DataBase;
using Sales.Infrastructure.DependencyInjection;

namespace IntegrationTests.Sales.Persistence;

public sealed class SalesCustomerReferenceNamesTests
{
    [Fact]
    public async Task CustomerRead_ResolvesNamesAndFiltersWholeListBeforePaging()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Host=localhost;Database=unused"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSalesInfrastructureServices(configuration, includeCatalogReadServices: false);
        services.RemoveAll<SalesDbContext>();
        services.RemoveAll<DbContextOptions<SalesDbContext>>();
        services.RemoveAll<IDbContextOptionsConfiguration<SalesDbContext>>();
        services.AddDbContext<SalesDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        var catalog = new RecordingCatalogReferences();
        services.AddSingleton<ICatalogReferenceDataReader>(catalog);

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.Counterparties.Add(Counterparty.Create("customer-1", Guid.NewGuid(), "Клієнт", "test@example.com", null, 1, now));
        db.Counterparties.AddRange(
            Counterparty.Create("match-2", Guid.NewGuid(), "Alpha Two", "two@example.com", "+38022", 1, now),
            Counterparty.Create("match-1", Guid.NewGuid(), "Alpha One", "one@example.com", "+38011", 1, now),
            Counterparty.Create("other", Guid.NewGuid(), "Bravo", "other@example.com", "+38033", 1, now));
        db.CounterpartyCategoryPriceTypes.AddRange(
            CounterpartyCategoryPriceType.Create("customer-1", 5513, 7),
            CounterpartyCategoryPriceType.Create("customer-1", 9999, 99));
        await db.SaveChangesAsync();

        var result = await scope.ServiceProvider.GetRequiredService<ICustomerReadService>()
            .GetByIdAsync("customer-1", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Роздрібна", result.Value.DefaultPriceTypeName);
        Assert.Equal("Замки", result.Value.CategoryPriceTypes[0].CategoryName);
        Assert.Equal("Оптова", result.Value.CategoryPriceTypes[0].PriceTypeName);
        Assert.Equal(9999, result.Value.CategoryPriceTypes[1].CategoryId);
        Assert.Null(result.Value.CategoryPriceTypes[1].CategoryName);
        Assert.Null(result.Value.CategoryPriceTypes[1].PriceTypeName);
        Assert.Equal(1, catalog.CategoryLookupCount);
        Assert.Equal(1, catalog.PriceTypeLookupCount);

        var list = await scope.ServiceProvider.GetRequiredService<ICustomerReadService>()
            .GetListAsync(new()
            {
                SearchTerm = "ALPHA",
                CounterpartyId = "match",
                Name = "alpha",
                Phone = "+380",
                Sort = "-name",
                Page = 2,
                PageSize = 1
            }, CancellationToken.None);

        Assert.True(list.IsSuccess);
        Assert.Equal(2, list.Value.PagedInfo.TotalRecords);
        Assert.Equal(2, list.Value.PagedInfo.TotalPages);
        Assert.Equal(["match-1"], list.Value.Value.Select(item => item.CounterpartyId));
    }

    private sealed class RecordingCatalogReferences : ICatalogReferenceDataReader
    {
        public int CategoryLookupCount { get; private set; }
        public int PriceTypeLookupCount { get; private set; }

        public Task<HashSet<int>> GetExistingCategoryIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
            => Task.FromResult(ids.ToHashSet());

        public Task<HashSet<int>> GetExistingPriceTypeIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
            => Task.FromResult(ids.ToHashSet());

        public Task<IReadOnlyDictionary<int, string>> GetCategoryNamesAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
        {
            CategoryLookupCount++;
            return Task.FromResult<IReadOnlyDictionary<int, string>>(new Dictionary<int, string> { [5513] = "Замки" });
        }

        public Task<IReadOnlyDictionary<int, string>> GetPriceTypeNamesAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
        {
            PriceTypeLookupCount++;
            return Task.FromResult<IReadOnlyDictionary<int, string>>(new Dictionary<int, string>
            {
                [1] = "Роздрібна",
                [7] = "Оптова"
            });
        }
    }
}
