using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Catalog.Application.Contracts.Persistence;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.DataBase;
using IntegrationTests.TestSupport.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests.Catalog.Api;

/// <summary>SC-006, SC-007, SC-008, SC-010, SC-012 and the Admin-only rules over HTTP, on an in-memory catalog.</summary>
public sealed class CatalogStableUrlsApiTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("PUT", "/api/categories/2/content/uk")]
    [InlineData("PUT", "/api/categories/2/slug")]
    [InlineData("PUT", "/api/admin/products/20/slug")]
    [InlineData("GET", "/api/categories?deleted=true")]
    [InlineData("GET", "/api/admin/products?deleted=true")]
    public async Task Manager_IsForbiddenFromAdminOnlyOperations(string method, string url)
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClientWithRole("Manager");

        using var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method == "PUT")
            request.Content = JsonContent.Create(new { slug = "nova-adresa" });
        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_SavesSanitizedContent_ManagerReadsIt_AndPublicPageShowsIt()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var admin = factory.CreateClientWithRole("Admin");
        using var manager = factory.CreateClientWithRole("Manager");

        using var save = await admin.PutAsJsonAsync("/api/categories/2/content/uk", new
        {
            seoTitle = "Замки врізні",
            bodyHtml = "<p onclick=\"x()\">Текст <strong>жирний</strong><script>alert(1)</script></p><a href=\"javascript:x\">x</a><a href=\"/uk/zamky\">ok</a>",
            faq = new[] { new { question = "Питання?", answer = "Відповідь." } }
        });
        Assert.Equal(HttpStatusCode.OK, save.StatusCode);
        using var saved = JsonDocument.Parse(await save.Content.ReadAsStringAsync());
        var body = saved.RootElement.GetProperty("bodyHtml").GetString()!;
        Assert.DoesNotContain("script", body);
        Assert.DoesNotContain("onclick", body);
        Assert.DoesNotContain("javascript", body);
        Assert.Contains("<strong>жирний</strong>", body);
        Assert.Contains("href=\"/uk/zamky\"", body);
        Assert.Equal("test-user", saved.RootElement.GetProperty("updatedBy").GetString());

        using var read = await manager.GetAsync("/api/categories/2/content");
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);

        using var page = await factory.CreateClient().GetAsync("/api/catalog/uk/categories/by-slug/zamky");
        using var pageBody = JsonDocument.Parse(await page.Content.ReadAsStringAsync());
        var content = pageBody.RootElement.GetProperty("content");
        Assert.Equal("Замки врізні", content.GetProperty("seoTitle").GetString());
        Assert.Equal("Питання?", content.GetProperty("faq")[0].GetProperty("question").GetString());
        Assert.Equal(JsonValueKind.Null,
            JsonDocument.Parse(await (await factory.CreateClient().GetAsync("/api/catalog/ru/categories/by-slug/zamky")).Content
                .ReadAsStringAsync()).RootElement.GetProperty("content").ValueKind);
    }

    [Fact]
    public async Task Admin_InvalidSlug_ReturnsBadRequest()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var admin = factory.CreateClientWithRole("Admin");

        using var invalid = await admin.PutAsJsonAsync("/api/categories/2/slug", new { slug = "Bad Slug" });
        using var taken = await admin.PutAsJsonAsync("/api/categories/2/slug", new { slug = "zamky-stari" });

        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, taken.StatusCode);
    }

    [Fact]
    public async Task PublicCategory_HistoryResolves_DeletedRedirectsToParent_UnknownIsNotFound()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClient();

        using var old = await client.GetAsync("/api/catalog/uk/categories/by-slug/zamki-2");
        using var oldBody = JsonDocument.Parse(await old.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, old.StatusCode);
        Assert.Equal("zamky", oldBody.RootElement.GetProperty("slug").GetString());
        Assert.Equal(2, oldBody.RootElement.GetProperty("productCount").GetInt32());
        Assert.Equal(100m, oldBody.RootElement.GetProperty("minPrice").GetDecimal());
        Assert.Equal(250m, oldBody.RootElement.GetProperty("maxPrice").GetDecimal());

        using var deleted = await client.GetAsync("/api/catalog/uk/categories/by-slug/staryi-rozdil");
        Assert.Equal(HttpStatusCode.NotFound, deleted.StatusCode);
        using var deletedBody = JsonDocument.Parse(await deleted.Content.ReadAsStringAsync());
        Assert.Equal("furnitura", deletedBody.RootElement.GetProperty("redirectSlug").GetString());

        using var unknown = await client.GetAsync("/api/catalog/uk/categories/by-slug/nemaie");
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);

        using var tree = await client.GetAsync("/api/catalog/uk/categories");
        using var treeBody = JsonDocument.Parse(await tree.Content.ReadAsStringAsync());
        var root = Assert.Single(treeBody.RootElement.EnumerateArray());
        Assert.Equal(2, root.GetProperty("productCount").GetInt32());
        Assert.Equal(["zamky"], root.GetProperty("children").EnumerateArray().Select(child => child.GetProperty("slug").GetString()));
    }

    [Fact]
    public async Task PublicProduct_HistoryResolves_DeletedPointsToCategory()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = factory.CreateClient();

        using var old = await client.GetAsync("/api/catalog/uk/product/zamok-a-20");
        using var oldBody = JsonDocument.Parse(await old.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, old.StatusCode);
        Assert.Equal("zamok-a", oldBody.RootElement.GetProperty("productSlug").GetString());

        using var deleted = await client.GetAsync("/api/catalog/uk/product/zamok-c");
        Assert.Equal(HttpStatusCode.NotFound, deleted.StatusCode);
        using var deletedBody = JsonDocument.Parse(await deleted.Content.ReadAsStringAsync());
        Assert.Equal("zamky", deletedBody.RootElement.GetProperty("redirectSlug").GetString());
        Assert.Equal("category", deletedBody.RootElement.GetProperty("redirectType").GetString());

        using var list = await client.GetAsync("/api/catalog/uk/nemaie/products");
        Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
    }

    private static CatalogDbContext Seed()
    {
        var db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        AddCategory(db, 1, "Фурнітура", "furnitura", "n1", null);
        AddCategory(db, 2, "Замки", "zamky", "n1.n2", 1);
        AddCategory(db, 3, "Порожня", "porozhnia", "n1.n3", 1);
        var gone = AddCategory(db, 4, "Старий розділ", "staryi-rozdil", "n1.n4", 1);
        gone.MarkDeleted(Now);
        db.SlugHistory.Add(SlugHistoryEntry.Create(CatalogEntityType.Category, "zamki-2", 2, Now));
        db.SlugHistory.Add(SlugHistoryEntry.Create(CatalogEntityType.Category, "zamky-stari", 3, Now));

        AddProduct(db, 20, "Замок А", "zamok-a", 100m);
        AddProduct(db, 21, "Замок Б", "zamok-b", 250m);
        AddProduct(db, 22, "Замок В", "zamok-c", 300m).MarkDeleted(Now);
        db.SlugHistory.Add(SlugHistoryEntry.Create(CatalogEntityType.Product, "zamok-a-20", 20, Now));

        db.SaveChanges();
        return db;
    }

    private static ProductCategory AddCategory(CatalogDbContext db, int id, string name, string slug, string path, int? parentId)
    {
        var category = ProductCategory.Create(ProductCategoryId.From(id), "000005513", name, slug, CategoryPath.From(path),
            parentId is null ? null : ProductCategoryId.From(parentId.Value));
        db.Categories.Add(category);
        return category;
    }

    private static Product AddProduct(CatalogDbContext db, int id, string name, string slug, decimal price)
    {
        var product = Product.Create(ProductId.From(id), "000005513", name, slug, ProductCategoryId.From(2),
            $"https://images-kedr.cdn.express/products/{id}.jpg", $"https://images-kedr.cdn.express/product-scheme/s{id}.jpg",
            Now, 5, 1, false, false, true);
        db.Products.Add(product);
        if (id != 22)
        {
            db.ProductListProjections.Add(ProductListProjection.Create(product.Id, name, name, slug, product.Photo,
                ProductCategoryId.From(2), "zamky", inStock: true, isSale: false, isNew: false, exportToSite: true, retailPrice: price));
        }

        return product;
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
            });
        });
}
