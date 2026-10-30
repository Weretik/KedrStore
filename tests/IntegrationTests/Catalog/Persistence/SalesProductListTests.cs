using Catalog.Application.Features.Products.GetSalesList;
using Catalog.Application.Integrations.OneC.Options;
using Catalog.Contracts.Pricing;
using Catalog.Contracts.Products.GetList;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaMoney;

namespace IntegrationTests.Catalog.Persistence;

/// <summary>Sales catalog page: category subtree, customer price rules, price filter, sorting, and paging.</summary>
public sealed class SalesProductListTests
{
    private const string Root = "000005513";
    private const int Retail = 12;
    private const int Wholesale = 7;

    [Fact]
    public async Task CategoryRulePricesItsProducts_OthersUseDefault_FilteredByPriceThenPaged()
    {
        await using var db = await SeedAsync();
        var handler = Handler(db);

        var result = await handler.Handle(new GetProductListQuery(new GetProductsRequest
        {
            PriceTypeRules = [new CategoryPriceTypeRule(2, Wholesale)],
            PriceFrom = 5,
            Sort = ProductSort.PriceAsc,
            Page = 1,
            PageSize = 2
        }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.PagedInfo.TotalRecords);
        // Product 12 is in category 2 and gets the wholesale price 6; the others keep the retail price.
        Assert.Equal([(12, 6m), (11, 10m)], result.Value.Value.Select(row => (row.Id, row.Price!.Value)));
    }

    [Fact]
    public async Task CategoryFilterIncludesDescendants_IdSortPagesBeforeJoiningPrices()
    {
        await using var db = await SeedAsync();
        var handler = Handler(db);

        var result = await handler.Handle(new GetProductListQuery(new GetProductsRequest
        {
            CategoryId = 1,
            Sort = ProductSort.IdDesc,
            Page = 1,
            PageSize = 10
        }), CancellationToken.None);

        Assert.Equal([13, 12, 11], result.Value.Value.Select(row => row.Id));
        Assert.Equal(3, result.Value.PagedInfo.TotalRecords);
        Assert.All(result.Value.Value, row => Assert.NotNull(row.Price));
    }

    private static GetProductListQueryHandler Handler(CatalogDbContext db) => new(
        db,
        new Snapshot<RootCategoryId>(new RootCategoryId
        {
            DoorsRootCategoryId = "000007226",
            HardwareRootCategoryId = Root,
            CosmosRootCategoryId = "Космос",
            CosmosCategoryId = 920000
        }),
        new Snapshot<CatalogPricingOptions>(new CatalogPricingOptions { RetailPriceTypeId = Retail }));

    private static async Task<CatalogDbContext> SeedAsync()
    {
        var db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        db.Categories.Add(ProductCategory.Create(ProductCategoryId.From(1), Root, "Root", "root", CategoryPath.Root("1")));
        db.Categories.Add(ProductCategory.Create(ProductCategoryId.From(2), Root, "Child", "child",
            CategoryPath.From("n1.n2"), ProductCategoryId.From(1)));
        db.Categories.Add(ProductCategory.Create(ProductCategoryId.From(3), Root, "Other", "other", CategoryPath.Root("3")));

        AddProduct(db, 11, 1, retail: 10);
        AddProduct(db, 12, 2, retail: 9, wholesale: 6);
        AddProduct(db, 13, 2, retail: 4);
        AddProduct(db, 14, 3, retail: 20);
        await db.SaveChangesAsync();
        return db;
    }

    private static void AddProduct(CatalogDbContext db, int id, int categoryId, decimal retail, decimal? wholesale = null)
    {
        var productId = ProductId.From(id);
        db.Products.Add(Product.Create(productId, Root, $"Product {id}", $"product-{id}",
            ProductCategoryId.From(categoryId), $"https://example.test/{id}.jpg", $"https://example.test/{id}-scheme.jpg", DateTimeOffset.UnixEpoch, stock: 5, qtyInPack: 1,
            isNew: false, isSale: false));
        db.ProductPrices.Add(ProductPrice.Create(Root, productId, PriceTypeId.From(Retail), new Money(retail, "UAH")));
        if (wholesale is not null)
        {
            db.ProductPrices.Add(ProductPrice.Create(Root, productId, PriceTypeId.From(Wholesale),
                new Money(wholesale.Value, "UAH")));
        }
    }

    private sealed class Snapshot<T>(T value) : IOptionsSnapshot<T> where T : class
    {
        public T Value => value;
        public T Get(string? name) => value;
    }
}
