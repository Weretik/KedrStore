using Catalog.Application.Features.Products.GetMissingPhotos;
using Catalog.Contracts.Products.GetMissingPhotos;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Catalog.Persistence;

public sealed class MissingProductPhotoReadTests
{
    [Fact]
    public async Task Handle_ReturnsOnlyUnavailableActiveProductsInPagedIdOrder()
    {
        await using var db = CreateContext();
        Seed(db, 1, check => check.RecordMissing(check.PhotoUrl, Now, 404), exportToSite: false);
        Seed(db, 2, check => check.RecordInvalidContentType(
            check.PhotoUrl, Now, 200, "invalid_content_type", "text/html"));
        Seed(db, 3, check => check.RecordCheckFailed(
            check.PhotoUrl, Now, 500, "http_500", "failure"));
        Seed(db, 4, check => check.RecordAvailable(check.PhotoUrl, Now, 200));
        Seed(db, 5, _ => { });
        var deleted = Seed(db, 6, check => check.RecordMissing(check.PhotoUrl, Now, 404));
        deleted.MarkAsDeleted(Now);
        await db.SaveChangesAsync();
        var handler = new GetMissingProductPhotosQueryHandler(db);

        var first = await handler.Handle(
            new GetMissingProductPhotosQuery(new GetMissingProductPhotosRequest { Page = 1, PageSize = 2 }),
            CancellationToken.None);
        var second = await handler.Handle(
            new GetMissingProductPhotosQuery(new GetMissingProductPhotosRequest { Page = 2, PageSize = 2 }),
            CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.Equal(3, first.Value.PagedInfo.TotalRecords);
        Assert.Equal(2, first.Value.PagedInfo.TotalPages);
        Assert.Equal([1, 2], first.Value.Value.Select(row => row.Id));
        Assert.Equal([ProductPhotoAvailabilityStatus.Missing, ProductPhotoAvailabilityStatus.InvalidContentType],
            first.Value.Value.Select(row => row.PhotoStatus));
        Assert.False(first.Value.Value[0].ExportToSite);
        Assert.All(first.Value.Value, row => Assert.False(row.PhotoAvailable));
        Assert.Single(second.Value.Value);
        Assert.Equal(3, second.Value.Value[0].Id);
        Assert.Equal(ProductPhotoAvailabilityStatus.CheckFailed, second.Value.Value[0].PhotoStatus);
    }

    [Fact]
    public async Task Handle_NoUnavailableProducts_ReturnsEmptySuccess()
    {
        await using var db = CreateContext();
        Seed(db, 1, check => check.RecordAvailable(check.PhotoUrl, Now, 200));
        await db.SaveChangesAsync();
        var handler = new GetMissingProductPhotosQueryHandler(db);

        var result = await handler.Handle(
            new GetMissingProductPhotosQuery(new GetMissingProductPhotosRequest()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Value);
        Assert.Equal(0, result.Value.PagedInfo.TotalRecords);
        Assert.Equal(0, result.Value.PagedInfo.TotalPages);
    }

    private static readonly DateTimeOffset Now = new(2026, 9, 23, 12, 0, 0, TimeSpan.Zero);

    private static CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CatalogDbContext(options);
    }

    private static Product Seed(
        CatalogDbContext db,
        int id,
        Action<ProductPhotoCheck> configureCheck,
        bool exportToSite = true)
    {
        var productId = ProductId.From(id);
        var categoryId = ProductCategoryId.From(id);
        var photo = $"https://images-kedr.cdn.express/products/{id}.jpg";
        db.Categories.Add(ProductCategory.Create(
            categoryId,
            "000005513",
            $"Category {id}",
            $"category-{id}",
            CategoryPath.From($"n{id}")));
        var product = Product.Create(
            productId,
            "000005513",
            $"Product {id}",
            $"product-{id}",
            categoryId,
            photo,
            $"https://images-kedr.cdn.express/product-scheme/s{id}.jpg",
            Now,
            id,
            1,
            false,
            false,
            exportToSite);
        var projection = ProductListProjection.Create(
            productId,
            $"Product {id} uk",
            $"Product {id} ru",
            $"product-{id}",
            photo,
            categoryId,
            $"category-{id}",
            id > 0,
            false,
            false,
            exportToSite,
            id * 10m);
        var check = ProductPhotoCheck.CreateUnknown(productId, photo);
        configureCheck(check);
        db.Products.Add(product);
        db.ProductListProjections.Add(projection);
        db.ProductPhotoChecks.Add(check);
        return product;
    }
}
