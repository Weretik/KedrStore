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

using IntegrationTests.TestSupport.Api;

namespace IntegrationTests.Catalog.Api;

public sealed class MissingProductPhotosApiTests
{
    [Fact]
    public async Task Get_ReturnsUnavailablePhotoPageForManager()
    {
        await using var db = CreateContext();
        SeedMissing(db, 42);
        await db.SaveChangesAsync();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClientWithRole("Manager");

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

    [Fact]
    public async Task Get_UsesServerFiltersBeforePagingAndCountsMatchingRows()
    {
        await using var db = CreateContext();
        SeedMissing(db, 10, inStock: false, isSale: false, isNew: true, exportToSite: false);
        SeedMissing(db, 11, inStock: false, isSale: false, isNew: true, exportToSite: false);
        SeedMissing(db, 12, inStock: true, isSale: false, isNew: true, exportToSite: false);
        await db.SaveChangesAsync();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClientWithRole("Manager");

        using var response = await client.GetAsync(
            "/api/admin/products/missing-photos?inStock=false&isSale=false&isNew=true&exportToSite=false&sort=IdDesc&page=2&pageSize=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(2, body.RootElement.GetProperty("pagedInfo").GetProperty("totalRecords").GetInt64());
        Assert.Equal(10, body.RootElement.GetProperty("value")[0].GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task AdminList_FiltersFalseBeforePaging_AndDetailsIncludeBothNamesAndFlags()
    {
        await using var db = CreateContext();
        SeedMissing(db, 20, inStock: false, isSale: true, isNew: false, exportToSite: false);
        SeedMissing(db, 21, inStock: true, isSale: true, isNew: false, exportToSite: false);
        await db.SaveChangesAsync();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClientWithRole("Manager");

        using var list = await client.GetAsync(
            "/api/admin/products?inStock=false&isSale=true&isNew=false&exportToSite=false&page=1&pageSize=1");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        using var listBody = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        Assert.Equal(1, listBody.RootElement.GetProperty("pagedInfo").GetProperty("totalRecords").GetInt64());
        Assert.Equal(20, listBody.RootElement.GetProperty("value")[0].GetProperty("id").GetInt32());

        using var details = await client.GetAsync("/api/admin/products/by-slug/product-20");
        Assert.Equal(HttpStatusCode.OK, details.StatusCode);
        using var detailBody = JsonDocument.Parse(await details.Content.ReadAsStringAsync());
        Assert.Equal("Product 20 uk", detailBody.RootElement.GetProperty("nameUk").GetString());
        Assert.Equal("Product 20 ru", detailBody.RootElement.GetProperty("nameRu").GetString());
        Assert.True(detailBody.RootElement.GetProperty("isSale").GetBoolean());
        Assert.False(detailBody.RootElement.GetProperty("inStock").GetBoolean());
        Assert.False(detailBody.RootElement.GetProperty("exportToSite").GetBoolean());
        Assert.Contains("s20.jpg", detailBody.RootElement.GetProperty("scheme").GetString());
    }

    [Fact]
    public async Task CategoryFilter_IncludesAllDescendants_AndSortsByUkrainianName()
    {
        await using var db = CreateContext();
        SeedMissing(db, 100);
        SeedMissing(db, 101);
        SeedMissing(db, 102);
        SeedMissing(db, 103);
        SeedMissing(db, 200);
        var categories = db.Categories.Local.ToDictionary(category => category.Id.Value);
        categories[101].Update("Child", "child", CategoryPath.From("n100.n101"), ProductCategoryId.From(100));
        categories[102].Update("Grandchild", "grandchild", CategoryPath.From("n100.n101.n102"), ProductCategoryId.From(101));
        categories[103].Update("Deep", "deep", CategoryPath.From("n100.n101.n102.n103"), ProductCategoryId.From(102));
        categories[100].UpdatePresentationMetadata("Якір", "Якорь", 0, 0);
        categories[101].UpdatePresentationMetadata("Блок", "Блок", 0, 1);
        categories[102].UpdatePresentationMetadata("Абетка", "Азбука", 0, 2);
        categories[103].UpdatePresentationMetadata("Вікно", "Окно", 0, 3);
        await db.SaveChangesAsync();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClientWithRole("Manager");

        foreach (var path in new[] { "/api/admin/products", "/api/admin/products/missing-photos" })
        {
            using var response = await client.GetAsync($"{path}?categoryId=100&sort=CategoryAsc&page=1&pageSize=2");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(4, body.RootElement.GetProperty("pagedInfo").GetProperty("totalRecords").GetInt64());
            var rows = body.RootElement.GetProperty("value");
            Assert.Equal([102, 101], rows.EnumerateArray().Select(row => row.GetProperty("id").GetInt32()));
            Assert.Equal("Абетка", rows[0].GetProperty("categoryName").GetString());
        }
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=101")]
    public async Task Get_InvalidPaging_ReturnsBadRequest(string query)
    {
        await using var db = CreateContext();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClientWithRole("Manager");

        using var response = await client.GetAsync($"/api/admin/products/missing-photos?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory(CatalogDbContext db)
        => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.AddTestRoleAuthentication();
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

    private static void SeedMissing(
        CatalogDbContext db,
        int id,
        bool inStock = true,
        bool isSale = false,
        bool isNew = false,
        bool exportToSite = false)
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
            isNew,
            isSale,
            exportToSite));
        db.ProductListProjections.Add(ProductListProjection.Create(
            productId,
            $"Product {id} uk",
            $"Product {id} ru",
            $"product-{id}",
            photo,
            categoryId,
            $"category-{id}",
            inStock,
            isSale,
            isNew,
            exportToSite,
            100m));
        var check = ProductPhotoCheck.CreateUnknown(productId, photo);
        check.RecordMissing(photo, now, 404);
        db.ProductPhotoChecks.Add(check);
    }
}
