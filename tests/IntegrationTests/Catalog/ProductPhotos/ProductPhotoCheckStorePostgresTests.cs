using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Products;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Catalog.ProductPhotos;

[Collection(CatalogPostgresCollection.Name)]
public sealed class ProductPhotoCheckStorePostgresTests(CatalogPostgresFixture postgres)
{
    [SkippableFact]
    public async Task GetActiveProductBatchAsync_PagesActiveProductsById()
    {
        postgres.SkipIfUnavailable();
        await using var db = postgres.CreateContext();
        await db.ProductPhotoChecks.ExecuteDeleteAsync();
        await db.Products.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.Categories.ExecuteDeleteAsync();

        SeedProduct(db, 1, exportToSite: true);
        SeedProduct(db, 2, exportToSite: false);
        var deleted = SeedProduct(db, 3, exportToSite: true);
        deleted.MarkAsDeleted(DateTimeOffset.UtcNow);
        SeedProduct(db, 4, exportToSite: true);
        await db.SaveChangesAsync();

        var store = new ProductPhotoCheckStore(db);
        var firstBatch = await store.GetActiveProductBatchAsync(0, 2, CancellationToken.None);
        var secondBatch = await store.GetActiveProductBatchAsync(2, 2, CancellationToken.None);
        var lastBatch = await store.GetActiveProductBatchAsync(4, 2, CancellationToken.None);

        Assert.Equal([1, 2], firstBatch.Select(product => product.ProductId.Value));
        Assert.Equal([4], secondBatch.Select(product => product.ProductId.Value));
        Assert.Empty(lastBatch);
    }

    private static Product SeedProduct(CatalogDbContext db, int id, bool exportToSite)
    {
        var categoryId = ProductCategoryId.From(id);
        db.Categories.Add(ProductCategory.Create(
            categoryId,
            "000005513",
            $"Category {id}",
            $"category-{id}",
            CategoryPath.From($"n{id}")));
        var product = Product.Create(
            ProductId.From(id),
            "000005513",
            $"Product {id}",
            $"product-{id}",
            categoryId,
            $"https://images-kedr.cdn.express/products/{id}.jpg",
            $"https://images-kedr.cdn.express/product-scheme/s{id}.jpg",
            DateTimeOffset.UtcNow,
            0,
            1,
            false,
            false,
            exportToSite);
        db.Products.Add(product);
        return product;
    }
}
