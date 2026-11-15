using Catalog.Application.Features.OneCSync.Mapping;
using Catalog.Contracts.Pricing;
using Catalog.Domain.Categories;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Prices;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Products;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NodaMoney;

namespace IntegrationTests.Catalog.Persistence;

/// <summary>
/// The product list rebuild changes only the rows that differ, in one transaction, and two rebuilds never run at once
/// (database rules 2.4, 3.2, 4; audit 2026-11-15 findings A and B).
/// </summary>
[Collection(CatalogPostgresCollection.Name)]
public sealed class ProductListRebuildPostgresTests(CatalogPostgresFixture postgres)
{
    private const string Root = "000005513";
    private const int RetailPriceTypeId = 12;
    private static readonly DateTimeOffset Now = new(2026, 11, 15, 0, 0, 0, TimeSpan.Zero);

    [SkippableFact]
    public async Task Rebuild_UpdatesChangedRows_AddsNew_RemovesGone_AndLeavesUnchangedRowsUntouched()
    {
        postgres.SkipIfUnavailable();
        await SeedAsync(productIds: [1, 2, 3]);
        await Rebuilder().RebuildAsync();
        var unchangedVersionBefore = await RowVersionAsync(1);

        await using (var db = postgres.CreateContext())
        {
            var renamed = await db.Products.SingleAsync(product => product.Id == ProductId.From(2));
            renamed.UpdateFromOneC("Товар 2 новий", ProductCategoryId.From(10), 1, true, false, false, Now);
            db.Products.Remove(await db.Products.SingleAsync(product => product.Id == ProductId.From(3)));
            db.Products.Add(NewProduct(4));
            await db.SaveChangesAsync();
        }

        await Rebuilder().RebuildAsync();

        await using var check = postgres.CreateContext();
        var rows = await check.ProductListProjections.AsNoTracking().OrderBy(row => row.ProductId).ToListAsync();
        Assert.Equal([1, 2, 4], rows.Select(row => row.ProductId.Value));
        Assert.Equal("Товар 2 новий", rows.Single(row => row.ProductId == ProductId.From(2)).NameUk);
        Assert.Equal(unchangedVersionBefore, await RowVersionAsync(1));
    }

    [SkippableFact]
    public async Task TwoRebuildsAtOnce_BothSucceed_AndLeaveOneRowPerProduct()
    {
        postgres.SkipIfUnavailable();
        await SeedAsync(productIds: Enumerable.Range(1, 300).ToArray());

        await Task.WhenAll(Rebuilder().RebuildAsync(), Rebuilder().RebuildAsync(), Rebuilder().RebuildAsync());

        await using var check = postgres.CreateContext();
        Assert.Equal(300, await check.ProductListProjections.CountAsync());
    }

    private async Task SeedAsync(int[] productIds)
    {
        await using var db = postgres.CreateContext();
        await db.Database.ExecuteSqlRawAsync(
            """TRUNCATE "CatalogProductGroups", "CatalogAttributes", "ProductMedia", "ProductListProjections", "ProductPrices", "ProductTranslations", "Products", "CatalogSlugHistory", "CategoryFaqItems", "CategoryContents", "ProductCategories" CASCADE;""");
        db.Categories.Add(ProductCategory.Create(ProductCategoryId.From(10), Root, "Завіси", "zavisy",
            CategoryPath.From("n10")).Value);
        foreach (var id in productIds)
            db.Products.Add(NewProduct(id));
        await db.SaveChangesAsync();
        foreach (var id in productIds)
            db.ProductPrices.Add(ProductPrice.Create(Root, ProductId.From(id), PriceTypeId.From(RetailPriceTypeId),
                new Money(id, "UAH")).Value);
        await db.SaveChangesAsync();
    }

    private static Product NewProduct(int id)
        => Product.Create(ProductId.From(id), Root, $"Товар {id}", $"tovar-{id}", ProductCategoryId.From(10), Now,
            5, 1, false, false, true).Value;

    private async Task<long> RowVersionAsync(int productId)
    {
        await using var db = postgres.CreateContext();
        return await db.Database.SqlQuery<long>(
                $"""SELECT xmin::text::bigint AS "Value" FROM "ProductListProjections" WHERE "ProductId" = {productId}""")
            .SingleAsync();
    }

    private ProductListProjectionRebuilder Rebuilder()
        => new(new Factory(postgres),
            new Snapshot<CatalogPricingOptions>(new CatalogPricingOptions { RetailPriceTypeId = RetailPriceTypeId }),
            new Snapshot<RootCategoryId>(new RootCategoryId
            {
                DoorsRootCategoryId = "000007226",
                HardwareRootCategoryId = Root,
                CosmosRootCategoryId = "Космос",
                CosmosCategoryId = 920000
            }),
            NullLogger<ProductListProjectionRebuilder>.Instance);

    private sealed class Factory(CatalogPostgresFixture postgres) : IDbContextFactory<CatalogDbContext>
    {
        public CatalogDbContext CreateDbContext() => postgres.CreateContext();
    }

    private sealed class Snapshot<T>(T value) : IOptionsSnapshot<T> where T : class
    {
        public T Value => value;
        public T Get(string? name) => value;
    }
}
