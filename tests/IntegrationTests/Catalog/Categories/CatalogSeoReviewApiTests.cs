using BuildingBlocks.Domain.Identifiers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Catalog.Application.Abstractions.Media;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Products.Shared;
using Catalog.Domain.Categories;
using Catalog.Domain.Products;
using Catalog.Infrastructure.Persistence;
using IntegrationTests.Catalog.ProductMedia;
using IntegrationTests.TestSupport.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests.Catalog.Categories;

/// <summary>Spec platform/010: the category part of the admin «SEO-огляд» over HTTP on an in-memory catalog.</summary>
public sealed class CatalogSeoReviewApiTests
{
    private const string Review = "/api/admin/catalog/seo-review";

    [Theory]
    [InlineData("Manager", HttpStatusCode.Forbidden)]
    [InlineData("Seo", HttpStatusCode.OK)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task Review_IsForTheSiteCatalogEditors(string role, HttpStatusCode expected)
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClientWithRole(role);

        using var response = await client.GetAsync(Review);

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Review_ListsShownCategoriesWithoutTexts_AndASavedTextLeavesItAtOnce()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var admin = factory.CreateClientWithRole("Admin");

        var before = await ItemsAsync(admin);
        using var save = await admin.PutAsJsonAsync("/api/categories/2/content/uk",
            new { seoTitle = "Замки", metaDescription = "Дверні замки оптом.", bodyHtml = "<p>Текст</p>" });
        var after = await ItemsAsync(admin);

        Assert.Contains(("category", 2L, "uk", "noText,noDescription"), before);
        Assert.Contains(("category", 2L, "ru", "noText,noDescription"), before);
        Assert.Equal(HttpStatusCode.OK, save.StatusCode);
        Assert.DoesNotContain(after, item => item is { Id: 2, Language: "uk" });
        Assert.Contains(after, item => item is { Id: 2, Language: "ru" });
    }

    [Fact]
    public async Task Review_LeavesOutACategoryClosedFromSearch()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var admin = factory.CreateClientWithRole("Admin");

        using var close = await admin.PutAsJsonAsync("/api/categories/1/search-settings", new { isClosedFromSearch = true });
        var items = await ItemsAsync(admin);

        Assert.Equal(HttpStatusCode.OK, close.StatusCode);
        Assert.Empty(items);
    }

    private static async Task<List<(string? Kind, long Id, string? Language, string Issues)>> ItemsAsync(HttpClient client)
    {
        using var json = JsonDocument.Parse(await client.GetStringAsync(Review));
        return json.RootElement.EnumerateArray()
            .Select(item => (item.GetProperty("kind").GetString(), item.GetProperty("id").GetInt64(),
                item.GetProperty("language").GetString(),
                string.Join(",", item.GetProperty("issues").EnumerateArray().Select(issue => issue.GetString()))))
            .ToList();
    }

    private static CatalogDbContext Seed()
    {
        var db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        AddCategory(db, 1, "Фурнітура", "furnitura", "n1", null);
        AddCategory(db, 2, "Замки", "zamky", "n1.n2", 1);
        var product = Product.Create(ProductId.Create(20), "000005513", "Замок А", "zamok-a", ProductCategoryId.Create(2),
            DateTimeOffset.UtcNow, 5, 1, false, false, true).Value;
        db.Products.Add(product);
        db.ProductListProjections.Add(ProductListProjection.Create(product.Id, "Замок А", "Замок А", "zamok-a",
            product.Photo, ProductCategoryId.Create(2), "zamky", inStock: true, isSale: false, isNew: false,
            exportToSite: true, retailPrice: 100m));
        db.SaveChanges();
        return db;
    }

    private static void AddCategory(CatalogDbContext db, int id, string name, string slug, string path, int? parentId)
    {
        var category = ProductCategory.Create(ProductCategoryId.Create(id), "000005513", name, slug,
            CategoryPath.Create(path), parentId is null ? null : ProductCategoryId.Create(parentId.Value)).Value;
        category.SetShownOnSite(true, DateTimeOffset.UtcNow);
        db.Categories.Add(category);
    }

    private static WebApplicationFactory<Program> CreateFactory(CatalogDbContext db)
        => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.AddTestRoleAuthentication();
                services.RemoveAll<IReadCatalogDbContext>();
                services.RemoveAll<CatalogDbContext>();
                services.AddSingleton(db);
                services.AddSingleton<IReadCatalogDbContext>(db);
                services.RemoveAll<IProductMediaLibrary>();
                services.AddSingleton<IProductMediaLibrary>(new FakeProductMediaLibrary());
            });
        });
}
