using Catalog.Application.Contracts.Persistence;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaMoney;
using Sales.Application.Contracts.Orders;
using Sales.Domain.Customers.Entities;
using Sales.Infrastructure.DataBase;
using Sales.Infrastructure.DependencyInjection;

namespace IntegrationTests.Sales.Persistence;

/// <summary>Customer prices and manager price adjustments in the admin order workspace.</summary>
public sealed class WorkspacePricingTests : IAsyncLifetime
{
    private const string CustomerId = "customer-1";
    private const int RetailType = 1;
    private const int DefaultType = 2;
    private const int LocksType = 3;

    private ServiceProvider _provider = null!;
    private AsyncServiceScope _scope;

    public async Task InitializeAsync()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Default"] = "Host=localhost;Database=unused" })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSalesInfrastructureServices(configuration, includeCatalogReadServices: false);
        services.RemoveAll<SalesDbContext>();
        services.RemoveAll<DbContextOptions<SalesDbContext>>();
        services.RemoveAll<IDbContextOptionsConfiguration<SalesDbContext>>();
        services.AddDbContext<SalesDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddDbContext<CatalogDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddScoped<IReadCatalogDbContext>(sp => sp.GetRequiredService<CatalogDbContext>());
        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateAsyncScope();

        var now = DateTimeOffset.UtcNow;
        var sales = _scope.ServiceProvider.GetRequiredService<SalesDbContext>();
        sales.Counterparties.Add(Counterparty.Create(CustomerId, Guid.NewGuid(), "Клієнт", "c@example.com", null, DefaultType, now));
        // The rule sits on the parent "Locks" category; products live in its child and grandchild.
        sales.CounterpartyCategoryPriceTypes.Add(CounterpartyCategoryPriceType.Create(CustomerId, 10, LocksType));
        await sales.SaveChangesAsync();

        var catalog = _scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        AddCategory(catalog, 10, null);
        AddCategory(catalog, 11, 10);
        AddCategory(catalog, 12, 11);
        AddCategory(catalog, 20, null);
        AddProduct(catalog, 101, 12, stock: 5, (RetailType, 300m), (DefaultType, 250m), (LocksType, 200m));
        AddProduct(catalog, 102, 20, stock: 5, (RetailType, 120m), (DefaultType, 100m), (LocksType, 90m));
        AddProduct(catalog, 103, 20, stock: 0, (RetailType, 80m), (DefaultType, 70m));
        AddProduct(catalog, 104, 20, stock: 5, (RetailType, 50m));
        await catalog.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
        await _provider.DisposeAsync();
    }

    private IWorkspacePricing Pricing => _scope.ServiceProvider.GetRequiredService<IWorkspacePricing>();

    [Fact]
    public async Task Quote_UsesCategoryRuleForSubcategories_AndDefaultTypeElsewhere()
    {
        var quote = await Pricing.QuoteAsync(new WorkspaceQuoteRequest(CustomerId,
            [new WorkspaceInputLine("101", 2), new WorkspaceInputLine("102", 1)]), CancellationToken.None);

        Assert.Equal([200m, 100m], quote.Lines.Select(x => x.UnitPrice));
        Assert.Equal(500m, quote.TotalAmount);
    }

    [Fact]
    public async Task Quote_AppliesPerUnitDiscountAndMarkup()
    {
        var quote = await Pricing.QuoteAsync(new WorkspaceQuoteRequest(CustomerId,
            [new WorkspaceInputLine("101", 2, -50m), new WorkspaceInputLine("102", 3, 10.5m)]), CancellationToken.None);

        var discounted = quote.Lines[0];
        Assert.Equal((200m, -50m, 150m, 300m),
            (discounted.BaseUnitPrice, discounted.PriceAdjustment, discounted.UnitPrice, discounted.LineAmount));
        Assert.Equal(331.5m, quote.Lines[1].LineAmount);
        Assert.Equal(631.5m, quote.TotalAmount);
    }

    [Fact]
    public async Task Quote_ChangesFingerprintWhenAdjustmentChanges()
    {
        var plain = await Pricing.QuoteAsync(new WorkspaceQuoteRequest(CustomerId, [new WorkspaceInputLine("101", 1)]), CancellationToken.None);
        var same = await Pricing.QuoteAsync(new WorkspaceQuoteRequest(CustomerId, [new WorkspaceInputLine("101", 1, 0.00m)]), CancellationToken.None);
        var discounted = await Pricing.QuoteAsync(new WorkspaceQuoteRequest(CustomerId, [new WorkspaceInputLine("101", 1, -1m)]), CancellationToken.None);

        Assert.Equal(plain.QuoteFingerprint, same.QuoteFingerprint);
        Assert.NotEqual(plain.QuoteFingerprint, discounted.QuoteFingerprint);
    }

    [Theory]
    [InlineData(-199.01, "price_below_minimum")]
    [InlineData(-200.01, "price_below_minimum")]
    [InlineData(0.001, "invalid_price_adjustment")]
    [InlineData(1_000_000.01, "invalid_price_adjustment")]
    public async Task Quote_RejectsInvalidAdjustments(double adjustment, string code)
    {
        var error = await Assert.ThrowsAsync<WorkspaceException>(() => Pricing.QuoteAsync(
            new WorkspaceQuoteRequest(CustomerId, [new WorkspaceInputLine("101", 1, (decimal)adjustment)]), CancellationToken.None));

        Assert.Equal(code, error.Code);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public async Task Quote_AllowsADiscountDownToOneHryvnia()
    {
        var quote = await Pricing.QuoteAsync(
            new WorkspaceQuoteRequest(CustomerId, [new WorkspaceInputLine("101", 2, -199m)]), CancellationToken.None);

        Assert.Equal(1m, quote.Lines[0].UnitPrice);
        Assert.Equal(2m, quote.TotalAmount);
    }

    [Fact]
    public async Task CustomerPrices_ReturnPricesRegardlessOfStock_AndNullWhenMissing()
    {
        var result = await Pricing.GetCustomerPricesAsync(
            new WorkspaceCustomerPricesRequest(CustomerId, ["101", "103", "104", "999"]), CancellationToken.None);

        Assert.Equal([("101", 200m), ("103", 70m), ("104", (decimal?)null), ("999", null)],
            result.Prices.Select(x => (x.ProductId, x.UnitPrice)));
    }

    [Theory]
    [InlineData("missing", 404)]
    [InlineData(" ", 400)]
    public async Task CustomerPrices_RejectUnknownOrEmptyCustomer(string customerId, int status)
    {
        var error = await Assert.ThrowsAsync<WorkspaceException>(() => Pricing.GetCustomerPricesAsync(
            new WorkspaceCustomerPricesRequest(customerId, ["101"]), CancellationToken.None));

        Assert.Equal(status, error.StatusCode);
    }

    private static void AddCategory(CatalogDbContext db, int id, int? parentId)
    {
        var path = parentId is null ? CategoryPath.Root(id.ToString()) : CategoryPath.From($"n{parentId}.n{id}");
        db.Categories.Add(ProductCategory.Create(ProductCategoryId.From(id), "000000001", $"Category {id}", $"category-{id}",
            path, parentId is null ? null : ProductCategoryId.From(parentId.Value)));
    }

    private static void AddProduct(CatalogDbContext db, int id, int categoryId, decimal stock,
        params (int Type, decimal Amount)[] prices)
    {
        db.Products.Add(Product.Create(ProductId.From(id), "000000001", $"Товар {id}", $"product-{id}",
            ProductCategoryId.From(categoryId), $"https://example.test/{id}.jpg", "scheme", DateTimeOffset.UtcNow, stock, 1, false, false));
        foreach (var (type, amount) in prices)
            db.ProductPrices.Add(ProductPrice.Create("000000001", ProductId.From(id), PriceTypeId.From(type),
                new Money(amount, "UAH")));
    }
}
