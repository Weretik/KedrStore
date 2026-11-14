using Catalog.Domain.Categories;
using Catalog.Domain.Categories.Content;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Slugs;
using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;
using Catalog.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using static IntegrationTests.Catalog.Import.CatalogImportTestSupport;

namespace IntegrationTests.Catalog.Import;

/// <summary>SC-001, SC-003, SC-004, SC-005, SC-013, SC-014 and media SC-34 on the product import.</summary>
public sealed class ProductImportSoftDeleteTests
{
    [Fact]
    public async Task Import_SoftDeletesMissing_CreatesWithStableSlug_AndKeepsCategoryForAmbiguousName()
    {
        await using var db = CreateContext();
        Seed(db, productCount: 10);
        var oneC = new FakeOneC
        {
            Products =
            [
                .. Enumerable.Range(1, 9).Select(id => Row(id, id == 1 ? "Ручки" : "Замки")),
                Row(100, "Замки", "Замок новий"),
                Row(101, "Невідома")
            ]
        };

        await ProductJob(db, oneC).RunAsync(Root, CancellationToken.None, rebuildProjection: false);

        var all = await db.Products.IgnoreQueryFilters().ToDictionaryAsync(product => product.Id.Value);
        Assert.True(all[10].IsDeleted);
        Assert.NotNull(all[10].DeletedAt);
        Assert.Equal("product-10", all[10].ProductSlug);
        Assert.Equal(10, all[1].CategoryId.Value);
        Assert.Equal("zamok-novyi", all[100].ProductSlug);
        Assert.False(all.ContainsKey(101));
        Assert.Equal(10, await db.Products.CountAsync());
    }

    [Fact]
    public async Task Import_KeepsMediaLinksOfKnownProducts_AndStartsNewProductsWithoutPhoto()
    {
        await using var db = CreateContext();
        Seed(db, productCount: 2);
        var linked = await db.Products.SingleAsync(product => product.Id == ProductId.From(1));
        linked.SetMedia([new ProductMediaFile(10, "https://images-kedr.cdn.express/products/1.jpg")],
            new ProductMediaFile(11, "https://images-kedr.cdn.express/product-scheme/s1.jpg"), DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();
        var oneC = new FakeOneC { Products = [Row(1, "Замки", "Нова назва"), Row(2, "Замки"), Row(100, "Замки")] };

        await ProductJob(db, oneC).RunAsync(Root, CancellationToken.None, rebuildProjection: false);

        db.ChangeTracker.Clear();
        var all = await db.Products.ToDictionaryAsync(product => product.Id.Value);
        Assert.Equal(2, all[1].MediaLinks.Count);
        Assert.Equal("https://images-kedr.cdn.express/products/1.jpg", all[1].Photo);
        Assert.Equal(string.Empty, all[100].Photo);
        Assert.Equal(string.Empty, all[100].Sсheme);
    }

    [Fact]
    public async Task Import_RestoresReturnedProductWithItsSlug()
    {
        await using var db = CreateContext();
        Seed(db, productCount: 10);
        var oneC = new FakeOneC { Products = [.. Enumerable.Range(1, 9).Select(id => Row(id, "Замки"))] };
        await ProductJob(db, oneC).RunAsync(Root, CancellationToken.None, rebuildProjection: false);

        oneC.Products = [.. Enumerable.Range(1, 10).Select(id => Row(id, "Замки"))];
        await ProductJob(db, oneC).RunAsync(Root, CancellationToken.None, rebuildProjection: false);

        var restored = await db.Products.SingleAsync(product => product.Id == ProductId.From(10));
        Assert.False(restored.IsDeleted);
        Assert.Null(restored.DeletedAt);
        Assert.Equal("product-10", restored.ProductSlug);
    }

    [Fact]
    public async Task Import_GuardBlocksDeletingMoreThanThirtyPercent()
    {
        await using var db = CreateContext();
        Seed(db, productCount: 10);
        var oneC = new FakeOneC { Products = [.. Enumerable.Range(1, 6).Select(id => Row(id, "Замки", "Змінена назва " + id))] };

        await ProductJob(db, oneC).RunAsync(Root, CancellationToken.None, rebuildProjection: false);

        Assert.Equal(10, await db.Products.CountAsync());
        Assert.Equal("Змінена назва 1", (await db.Products.SingleAsync(product => product.Id == ProductId.From(1))).Name);
    }

    [Fact]
    public async Task Import_SecondRunWithSameDataWritesNothing()
    {
        await using var db = CreateContext();
        Seed(db, productCount: 3);
        var oneC = new FakeOneC { Products = [.. Enumerable.Range(1, 3).Select(id => Row(id, "Замки", "Новий " + id))] };
        await ProductJob(db, oneC).RunAsync(Root, CancellationToken.None, rebuildProjection: false);
        var firstUpdatedAt = (await db.Products.SingleAsync(product => product.Id == ProductId.From(1))).UpdatedAt;

        await ProductJob(db, oneC).RunAsync(Root, CancellationToken.None, rebuildProjection: false);

        Assert.NotNull(firstUpdatedAt);
        Assert.Equal(firstUpdatedAt, (await db.Products.SingleAsync(product => product.Id == ProductId.From(1))).UpdatedAt);
    }

    [Fact]
    public async Task Import_NewProductGetsCollisionSuffixAgainstHistory()
    {
        await using var db = CreateContext();
        Seed(db, productCount: 1);
        db.SlugHistory.Add(SlugHistoryEntry.Create(CatalogEntityType.Product, "zamok", 1, DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();
        var oneC = new FakeOneC { Products = [Row(1, "Замки"), Row(200, "Замки", "Замок")] };

        await ProductJob(db, oneC).RunAsync(Root, CancellationToken.None, rebuildProjection: false);

        Assert.Equal("zamok-2", (await db.Products.SingleAsync(product => product.Id == ProductId.From(200))).ProductSlug);
    }

    private static void Seed(CatalogDbContext db, int productCount)
    {
        db.Categories.Add(ProductCategory.Create(ProductCategoryId.From(10), Root, "Замки", "zamky", CategoryPath.From("n10")));
        db.Categories.Add(ProductCategory.Create(ProductCategoryId.From(11), Root, "Ручки", "ruchky", CategoryPath.From("n11")));
        db.Categories.Add(ProductCategory.Create(ProductCategoryId.From(12), Root, "ручки", "ruchky-2", CategoryPath.From("n12")));
        for (var id = 1; id <= productCount; id++)
        {
            db.Products.Add(Product.Create(ProductId.From(id), Root, $"Product {id}", $"product-{id}", ProductCategoryId.From(10),
                DateTimeOffset.UtcNow.AddDays(-1), 0, 1, false, false, true));
        }

        db.SaveChanges();
    }

    private static OneCProductDto Row(int id, string category, string? name = null)
        => new(id, name ?? $"Product {id}", category, "", false, false, true, 1);
}
