using Catalog.Application.Features.Products.GetMissingPhotos;
using Catalog.Contracts.Products.GetMissingPhotos;
using Catalog.Contracts.Products.GetList;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.DataBase;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Catalog.Persistence;

[Collection(CatalogPostgresCollection.Name)]
public sealed class MissingProductPhotoPostgresReadTests(CatalogPostgresFixture postgres)
{
    [SkippableFact]
    public async Task Handle_SortsAndPagesUnavailablePhotosInPostgres()
    {
        postgres.SkipIfUnavailable();
        await using var db = postgres.CreateContext();
        await db.ProductListProjections.ExecuteDeleteAsync();
        await db.ProductPhotoChecks.ExecuteDeleteAsync();
        await db.Products.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.Categories.ExecuteDeleteAsync();

        SeedMissing(db, 7201);
        SeedMissing(db, 7202);
        SeedMissing(db, 7203);
        await db.SaveChangesAsync();

        var handler = new GetMissingProductPhotosQueryHandler(db);
        var ascending = await handler.Handle(
            new GetMissingProductPhotosQuery(new GetMissingProductPhotosRequest { Page = 1, PageSize = 2 }),
            CancellationToken.None);
        var descending = await handler.Handle(
            new GetMissingProductPhotosQuery(new GetMissingProductPhotosRequest
            {
                Page = 2,
                PageSize = 2,
                Sort = ProductSort.IdDesc
            }),
            CancellationToken.None);

        Assert.True(ascending.IsSuccess);
        Assert.Equal(3, ascending.Value.PagedInfo.TotalRecords);
        Assert.Equal([7201, 7202], ascending.Value.Value.Select(row => row.Id));
        Assert.True(descending.IsSuccess);
        Assert.Equal(3, descending.Value.PagedInfo.TotalRecords);
        Assert.Equal([7201], descending.Value.Value.Select(row => row.Id));
    }

    private static void SeedMissing(CatalogDbContext db, int id)
    {
        var productId = ProductId.From(id);
        var categoryId = ProductCategoryId.From(id);
        var now = DateTimeOffset.UtcNow;
        var photo = $"https://images-kedr.cdn.express/products/{id}.jpg";
        db.Categories.Add(ProductCategory.Create(
            categoryId, "000005513", $"Category {id}", $"category-{id}", CategoryPath.From($"n{id}")));
        db.Products.Add(Product.Create(
            productId, "000005513", $"Product {id}", $"product-{id}", categoryId, photo,
            $"https://images-kedr.cdn.express/product-scheme/s{id}.jpg", now, 1, 1,
            false, false, true));
        db.ProductListProjections.Add(ProductListProjection.Create(
            productId, $"Product {id} uk", $"Product {id} ru", $"product-{id}", photo,
            categoryId, $"category-{id}", true, false, false, true, id));
        var check = ProductPhotoCheck.CreateUnknown(productId, photo);
        check.RecordMissing(photo, now, 404);
        db.ProductPhotoChecks.Add(check);
    }
}
