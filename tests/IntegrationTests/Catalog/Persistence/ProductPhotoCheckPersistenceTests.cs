using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.DataBase;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IntegrationTests.Catalog.Persistence;

[Collection(CatalogPostgresCollection.Name)]
public sealed class ProductPhotoCheckPersistenceTests(CatalogPostgresFixture postgres)
{
    [Fact]
    public void CatalogModel_EnforcesPhotoCheckStateConstraints()
    {
        using var db = CreateContext();
        var entity = db.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(ProductPhotoCheck));

        Assert.NotNull(entity);
        Assert.Equal(2, entity.GetCheckConstraints().Count());
        Assert.Contains(entity.GetCheckConstraints(), constraint => constraint.Name == "CK_ProductPhotoChecks_Status");
        Assert.Contains(entity.GetCheckConstraints(), constraint => constraint.Name == "CK_ProductPhotoChecks_State");
    }

    [SkippableFact]
    public async Task PhysicalProductDelete_CascadesPhotoCheck()
    {
        postgres.SkipIfUnavailable();
        await ResetDatabaseAsync();
        await SeedProductAndCheckAsync(7001);

        await using (var db = postgres.CreateContext())
        {
            await db.Products.IgnoreQueryFilters().Where(product => product.Id == ProductId.From(7001)).ExecuteDeleteAsync();
        }

        await using var verificationDb = postgres.CreateContext();
        Assert.Empty(await verificationDb.ProductPhotoChecks.ToListAsync());
    }

    [SkippableFact]
    public async Task SoftDeletedProduct_PreservesPhotoCheck()
    {
        postgres.SkipIfUnavailable();
        await ResetDatabaseAsync();
        await SeedProductAndCheckAsync(7002);

        await using (var db = postgres.CreateContext())
        {
            var product = await db.Products.SingleAsync(product => product.Id == ProductId.From(7002));
            product.MarkAsDeleted(DateTimeOffset.UtcNow);
            await db.SaveChangesAsync();
        }

        await using var verificationDb = postgres.CreateContext();
        Assert.Empty(await verificationDb.Products.ToListAsync());
        Assert.Single(await verificationDb.ProductPhotoChecks.ToListAsync());
    }

    [SkippableFact]
    public async Task PhotoCheck_RequiresExistingProduct()
    {
        postgres.SkipIfUnavailable();
        await ResetDatabaseAsync();
        await using var db = postgres.CreateContext();
        db.ProductPhotoChecks.Add(ProductPhotoCheck.CreateUnknown(
            ProductId.From(7999),
            "https://images-kedr.cdn.express/products/7999.jpg"));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    private static CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql("Host=localhost;Database=kedr_test;Username=kedr_user;Password=not-used")
            .Options;

        return new CatalogDbContext(options);
    }

    private async Task SeedProductAndCheckAsync(int productId)
    {
        await using var db = postgres.CreateContext();
        var categoryId = ProductCategoryId.From(productId);
        db.Categories.Add(ProductCategory.Create(
            categoryId,
            "000005513",
            $"Category {productId}",
            $"category-{productId}",
            global::Catalog.Domain.ValueObjects.CategoryPath.From($"n{productId}")));
        db.Products.Add(Product.Create(
            ProductId.From(productId),
            "000005513",
            $"Product {productId}",
            $"product-{productId}",
            categoryId,
            $"https://images-kedr.cdn.express/products/{productId}.jpg",
            $"https://images-kedr.cdn.express/product-scheme/s{productId}.jpg",
            DateTimeOffset.UtcNow,
            0,
            1,
            false,
            false,
            true));
        db.ProductPhotoChecks.Add(ProductPhotoCheck.CreateUnknown(
            ProductId.From(productId),
            $"https://images-kedr.cdn.express/products/{productId}.jpg"));
        await db.SaveChangesAsync();
    }

    private async Task ResetDatabaseAsync()
    {
        await using var db = postgres.CreateContext();
        await db.ProductPhotoChecks.ExecuteDeleteAsync();
        await db.Products.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.Categories.ExecuteDeleteAsync();
    }
}
