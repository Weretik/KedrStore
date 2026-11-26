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

/// <summary>
/// Spec catalog/011 over HTTP on an in-memory catalog: «Закрити від пошуку» with subcategories, the admin's site names,
/// and the link-preview picture, as the admin saves them and the public site reads them.
/// </summary>
public sealed class CategorySearchSettingsApiTests
{
    [Theory]
    [InlineData("/api/categories/1/search-settings")]
    [InlineData("/api/categories/1/site-names")]
    public async Task Manager_IsForbidden(string url)
    {
        await using var db = Seed();
        using var factory = CreateFactory(db, new FakeProductMediaLibrary());
        using var manager = factory.CreateClientWithRole("Manager");

        using var response = await manager.PutAsJsonAsync(url, new { isClosedFromSearch = true });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_ClosesAParent_ThenItAndItsSubcategoryAreClosedOnTheSite_SC01()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db, new FakeProductMediaLibrary());
        using var admin = factory.CreateClientWithRole("Admin");

        using var close = await admin.PutAsJsonAsync("/api/categories/1/search-settings", new { isClosedFromSearch = true });
        Assert.Equal(HttpStatusCode.OK, close.StatusCode);
        using var saved = JsonDocument.Parse(await close.Content.ReadAsStringAsync());
        Assert.True(saved.RootElement.GetProperty("isClosedFromSearch").GetBoolean());

        var site = factory.CreateClient();
        using var page = JsonDocument.Parse(await site.GetStringAsync("/api/catalog/uk/categories/by-slug/zamky"));
        Assert.True(page.RootElement.GetProperty("isClosedFromSearch").GetBoolean());
        Assert.NotEqual(JsonValueKind.Null, page.RootElement.GetProperty("updatedAt").ValueKind);
        Assert.True(await db.Categories.AsNoTracking().Where(category => category.Id.Value == 3)
            .Select(category => category.IsClosedFromSearch).SingleAsync());

        using var open = await admin.PutAsJsonAsync("/api/categories/1/search-settings", new { isClosedFromSearch = false });
        Assert.Equal(HttpStatusCode.OK, open.StatusCode);
        Assert.False(await db.Categories.AsNoTracking().Where(category => category.Id.Value == 2)
            .Select(category => category.IsClosedFromSearch).SingleAsync());
    }

    [Fact]
    public async Task Admin_WritesSiteNames_TheSiteShowsThem_AndABlankNameGoesBackToOneC_SC02()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db, new FakeProductMediaLibrary());
        using var admin = factory.CreateClientWithRole("Admin");

        using var set = await admin.PutAsJsonAsync("/api/categories/2/site-names",
            new { nameUk = "Дверні замки", nameRu = "Дверные замки" });
        Assert.Equal(HttpStatusCode.OK, set.StatusCode);

        var site = factory.CreateClient();
        using var uk = JsonDocument.Parse(await site.GetStringAsync("/api/catalog/uk/categories/by-slug/zamky"));
        Assert.Equal("Дверні замки", uk.RootElement.GetProperty("name").GetString());
        using var ru = JsonDocument.Parse(await site.GetStringAsync("/api/catalog/ru/categories/by-slug/zamky"));
        Assert.Equal("Дверные замки", ru.RootElement.GetProperty("name").GetString());
        Assert.Equal("Дверные замки", ru.RootElement.GetProperty("breadcrumbs").EnumerateArray().Last()
            .GetProperty("name").GetString());

        using var clear = await admin.PutAsJsonAsync("/api/categories/2/site-names", new { nameUk = " ", nameRu = (string?)null });
        using var cleared = JsonDocument.Parse(await clear.Content.ReadAsStringAsync());
        Assert.Equal("Замки", cleared.RootElement.GetProperty("shortNameUk").GetString());
        Assert.Equal(JsonValueKind.Null, cleared.RootElement.GetProperty("siteNameUk").ValueKind);

        using var tooLong = await admin.PutAsJsonAsync("/api/categories/2/site-names", new { nameUk = new string('я', 101) });
        Assert.Equal(HttpStatusCode.BadRequest, tooLong.StatusCode);
        Assert.Contains("nameUk", await tooLong.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Admin_PicksAShareImage_TheSiteGetsItsAddress_AndAFileOutsideTheLibraryIsRefused_SC03()
    {
        await using var db = Seed();
        var media = new FakeProductMediaLibrary();
        media.Add(70, "seo/zamky.webp");
        using var factory = CreateFactory(db, media);
        using var admin = factory.CreateClientWithRole("Admin");

        using var missing = await admin.PutAsJsonAsync("/api/categories/2/search-settings",
            new { isClosedFromSearch = false, shareImageMediaFileId = 99 });
        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
        Assert.Contains("shareImageMediaFileId", await missing.Content.ReadAsStringAsync());

        using var set = await admin.PutAsJsonAsync("/api/categories/2/search-settings",
            new { isClosedFromSearch = false, shareImageMediaFileId = 70 });
        Assert.Equal(HttpStatusCode.OK, set.StatusCode);
        using var byId = JsonDocument.Parse(await admin.GetStringAsync("/api/categories/2"));
        Assert.Equal("https://images-kedr.cdn.express/seo/zamky.webp", byId.RootElement.GetProperty("shareImageUrl").GetString());

        using var page = JsonDocument.Parse(await factory.CreateClient()
            .GetStringAsync("/api/catalog/uk/categories/by-slug/zamky"));
        Assert.Equal("https://images-kedr.cdn.express/seo/zamky.webp", page.RootElement.GetProperty("shareImageUrl").GetString());
        Assert.False(page.RootElement.GetProperty("isClosedFromSearch").GetBoolean());
    }

    [Fact]
    public async Task PublicTree_TakesTheLaterOfTheCategoryAndItsTextChange_R04()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db, new FakeProductMediaLibrary());
        using var admin = factory.CreateClientWithRole("Admin");
        using var save = await admin.PutAsJsonAsync("/api/categories/2/content/uk", new { seoTitle = "Замки" });
        Assert.Equal(HttpStatusCode.OK, save.StatusCode);
        var textChange = (await db.CategoryContents.AsNoTracking().SingleAsync()).UpdatedAt;

        using var tree = JsonDocument.Parse(await factory.CreateClient().GetStringAsync("/api/catalog/uk/categories"));

        var node = Flatten(tree.RootElement).Single(item => item.GetProperty("id").GetInt32() == 2);
        Assert.Equal(textChange, node.GetProperty("updatedAt").GetDateTimeOffset());
    }

    private static IEnumerable<JsonElement> Flatten(JsonElement nodes)
        => nodes.EnumerateArray().SelectMany(node => Flatten(node.GetProperty("children")).Prepend(node));

    private static CatalogDbContext Seed()
    {
        var db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        AddCategory(db, 1, "Фурнітура", "furnitura", "n1", null);
        AddCategory(db, 2, "Замки", "zamky", "n1.n2", 1);
        AddCategory(db, 3, "Врізні", "vrizni", "n1.n2.n3", 2);
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

    private static WebApplicationFactory<Program> CreateFactory(CatalogDbContext db, FakeProductMediaLibrary media)
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
                services.AddSingleton<IProductMediaLibrary>(media);
            });
        });
}
