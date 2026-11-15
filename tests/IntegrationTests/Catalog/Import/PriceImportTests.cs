using Catalog.Application.Features.OneCSync.Source.Prices;
using Catalog.Domain.Categories;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Prices;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NodaMoney;
using static IntegrationTests.Catalog.Import.CatalogImportTestSupport;

namespace IntegrationTests.Catalog.Import;

/// <summary>
/// The 1C price import reads what it needs in a few queries and saves once (database rules 1.2 and 3.1; audit
/// 2026-11-15 finding C): prices are added, changed and removed together, and a price of an unknown product is skipped.
/// </summary>
public sealed class PriceImportTests
{
    private const int Retail = 12;
    private const int Wholesale = 14;

    [Fact]
    public async Task Import_AddsChangesAndRemovesPrices_InOneSave_AndSkipsUnknownProducts()
    {
        var saves = new SaveCounter();
        await using var db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).AddInterceptors(saves).Options);
        Seed(db);
        saves.Count = 0;
        var oneC = new FakeOneC
        {
            Prices =
            [
                new OneCPriceDto(1, Retail, 150),
                new OneCPriceDto(2, Retail, 200),
                new OneCPriceDto(2, Wholesale, 180),
                new OneCPriceDto(2, Wholesale, 170),
                new OneCPriceDto(999, Retail, 10)
            ]
        };

        await PricesJob(db, oneC).RunAsync(Root, CancellationToken.None, rebuildProjection: false);

        var prices = await db.ProductPrices.AsNoTracking()
            .Select(price => new { Product = price.ProductId.Value, Type = price.PriceTypeId.Value, price.Amount })
            .OrderBy(price => price.Product).ThenBy(price => price.Type)
            .ToListAsync();
        Assert.Equal(
            [new { Product = 1, Type = Retail, Amount = 150m }, new { Product = 2, Type = Retail, Amount = 200m },
                new { Product = 2, Type = Wholesale, Amount = 170m }],
            prices);
        Assert.Equal(1, saves.Count);
    }

    private static void Seed(CatalogDbContext db)
    {
        db.Categories.Add(ProductCategory.Create(ProductCategoryId.From(10), Root, "Замки", "zamky",
            CategoryPath.From("n10")).Value);
        foreach (var id in new[] { 1, 2 })
        {
            db.Products.Add(Product.Create(ProductId.From(id), Root, $"Product {id}", $"product-{id}",
                ProductCategoryId.From(10), DateTimeOffset.UtcNow, 0, 1, false, false, true).Value);
        }

        db.ProductPrices.Add(Price(1, Retail, 100));
        db.ProductPrices.Add(Price(1, Wholesale, 90));
        db.SaveChanges();
    }

    private static ProductPrice Price(int productId, int priceTypeId, decimal amount)
        => ProductPrice.Create(Root, ProductId.From(productId), PriceTypeId.From(priceTypeId),
            new Money(amount, "UAH")).Value;

    private sealed class SaveCounter : SaveChangesInterceptor
    {
        public int Count { get; set; }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Count++;
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
