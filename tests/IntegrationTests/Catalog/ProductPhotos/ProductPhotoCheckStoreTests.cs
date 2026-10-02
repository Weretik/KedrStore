using Catalog.Application.Contracts.Integrations;
using Catalog.Application.Contracts.Persistence;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Products;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Catalog.ProductPhotos;

public sealed class ProductPhotoCheckStoreTests
{
    [Fact]
    public async Task GetActiveProductBatchAsync_IncludesBothExportFlagsAndExcludesSoftDeleted()
    {
        await using var db = CreateContext();
        SeedProduct(db, 1, exportToSite: true);
        SeedProduct(db, 2, exportToSite: false);
        var deleted = SeedProduct(db, 3, exportToSite: true);
        deleted.MarkAsDeleted(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();
        var store = new ProductPhotoCheckStore(db);

        var batch = await store.GetActiveProductBatchAsync(0, 10, CancellationToken.None);

        Assert.Equal([1, 2], batch.Select(product => product.ProductId.Value));
    }

    [Fact]
    public async Task ReconcileAsync_CreatesUnknownAndResetsChangedUrl()
    {
        await using var db = CreateContext();
        SeedProduct(db, 1, exportToSite: true);
        SeedProduct(db, 2, exportToSite: true);
        var existing = ProductPhotoCheck.CreateUnknown(ProductId.From(1), PhotoUrl(1));
        existing.RecordMissing(existing.PhotoUrl, DateTimeOffset.UtcNow, 404);
        db.ProductPhotoChecks.Add(existing);
        await db.SaveChangesAsync();
        var store = new ProductPhotoCheckStore(db);

        await store.ReconcileAsync(
            [new(ProductId.From(1), PhotoUrl(11)), new(ProductId.From(2), PhotoUrl(2))],
            CancellationToken.None);

        var checks = await db.ProductPhotoChecks.OrderBy(check => check.Id).ToListAsync();
        Assert.Equal(2, checks.Count);
        Assert.All(checks, check => Assert.Equal(ProductPhotoStatus.Unknown, check.Status));
        Assert.Equal(PhotoUrl(11), checks[0].PhotoUrl);
    }

    [Fact]
    public async Task SaveResultsAsync_OldUrlIsDiscardedAsStale()
    {
        await using var db = CreateContext();
        SeedProduct(db, 1, exportToSite: true);
        db.ProductPhotoChecks.Add(ProductPhotoCheck.CreateUnknown(ProductId.From(1), PhotoUrl(11)));
        await db.SaveChangesAsync();
        var store = new ProductPhotoCheckStore(db);
        var update = new ProductPhotoCheckUpdate(
            ProductId.From(1),
            PhotoUrl(1),
            new ProductPhotoProbeResult(ProductPhotoStatus.Missing, 404, "http_missing", null));

        var stale = await store.SaveResultsAsync([update], DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.Equal(1, stale);
        var check = await db.ProductPhotoChecks.SingleAsync();
        Assert.Equal(ProductPhotoStatus.Unknown, check.Status);
        Assert.Equal(PhotoUrl(11), check.PhotoUrl);
    }

    [Fact]
    public async Task SaveResultsAsync_CurrentUrlPersistsTerminalState()
    {
        await using var db = CreateContext();
        SeedProduct(db, 1, exportToSite: true);
        db.ProductPhotoChecks.Add(ProductPhotoCheck.CreateUnknown(ProductId.From(1), PhotoUrl(1)));
        await db.SaveChangesAsync();
        var store = new ProductPhotoCheckStore(db);
        var update = new ProductPhotoCheckUpdate(
            ProductId.From(1),
            PhotoUrl(1),
            new ProductPhotoProbeResult(ProductPhotoStatus.Available, 200, null, null));

        var stale = await store.SaveResultsAsync([update], DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.Equal(0, stale);
        var check = await db.ProductPhotoChecks.SingleAsync();
        Assert.Equal(ProductPhotoStatus.Available, check.Status);
        Assert.Equal(200, check.HttpStatusCode);
    }

    private static CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CatalogDbContext(options);
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
            PhotoUrl(id),
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

    private static string PhotoUrl(int id)
        => $"https://images-kedr.cdn.express/products/{id}.jpg";
}
