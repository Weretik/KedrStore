using System.Net;
using System.Text.Json;
using Catalog.Application.Contracts.Persistence;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests.Catalog.Api;

public sealed class MissingProductPhotosApiTests
{
    [Fact]
    public async Task Get_ReturnsAnonymousUnavailablePhotoPage()
    {
        await using var db = CreateContext();
        SeedMissing(db, 42);
        await db.SaveChangesAsync();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/admin/products/missing-photos?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(1, body.RootElement.GetProperty("pagedInfo").GetProperty("totalRecords").GetInt64());
        var row = body.RootElement.GetProperty("value")[0];
        Assert.Equal(42, row.GetProperty("id").GetInt32());
        Assert.False(row.GetProperty("photoAvailable").GetBoolean());
        Assert.Equal("Missing", row.GetProperty("photoStatus").GetString());
        Assert.Equal(404, row.GetProperty("photoHttpStatusCode").GetInt32());
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=101")]
    public async Task Get_InvalidPaging_ReturnsBadRequest(string query)
    {
        await using var db = CreateContext();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync($"/api/admin/products/missing-photos?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory(CatalogDbContext db)
        => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IReadCatalogDbContext>();
                services.AddSingleton<IReadCatalogDbContext>(db);
            });
        });

    private static CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CatalogDbContext(options);
    }

    private static void SeedMissing(CatalogDbContext db, int id)
    {
        var now = new DateTimeOffset(2026, 9, 23, 12, 0, 0, TimeSpan.Zero);
        var productId = ProductId.From(id);
        var categoryId = ProductCategoryId.From(id);
        var photo = $"https://images-kedr.cdn.express/products/{id}.jpg";
        db.Categories.Add(ProductCategory.Create(
            categoryId,
            "000005513",
            $"Category {id}",
            $"category-{id}",
            CategoryPath.From($"n{id}")));
        db.Products.Add(Product.Create(
            productId,
            "000005513",
            $"Product {id}",
            $"product-{id}",
            categoryId,
            photo,
            $"https://images-kedr.cdn.express/product-scheme/s{id}.jpg",
            now,
            2,
            1,
            false,
            false,
            false));
        db.ProductListProjections.Add(ProductListProjection.Create(
            productId,
            $"Product {id} uk",
            $"Product {id} ru",
            $"product-{id}",
            photo,
            categoryId,
            $"category-{id}",
            true,
            false,
            false,
            false,
            100m));
        var check = ProductPhotoCheck.CreateUnknown(productId, photo);
        check.RecordMissing(photo, now, 404);
        db.ProductPhotoChecks.Add(check);
    }
}
