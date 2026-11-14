using System.Net;
using System.Text.Json;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Domain.Categories;
using Catalog.Domain.Categories.Content;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Slugs;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;
using Catalog.Infrastructure.Persistence;
using IntegrationTests.TestSupport.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests.Catalog.Api;

/// <summary>Catalog block of the admin «Огляд» over HTTP on an in-memory catalog.</summary>
public sealed class AdminCatalogOverviewApiTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Overview_CountsProductsWithoutPhoto_AndActiveCategoriesMissingALanguage()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var json = JsonDocument.Parse(await manager.GetStringAsync("/api/admin/catalog/overview"));

        var root = json.RootElement;
        Assert.Equal(4, root.GetProperty("productsWithoutPhoto").GetInt32());
        var top = root.GetProperty("topCategoryWithoutPhoto");
        Assert.Equal(2, top.GetProperty("id").GetInt32());
        Assert.Equal("Замки", top.GetProperty("name").GetString());
        Assert.Equal(3, top.GetProperty("count").GetInt32());
        Assert.Equal(2, root.GetProperty("categoriesWithoutText").GetInt32());
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task Overview_IsForManagersAndAdmins(string? role, HttpStatusCode expected)
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        using var response = await client.GetAsync("/api/admin/catalog/overview");

        Assert.Equal(expected, response.StatusCode);
    }

    private static CatalogDbContext Seed()
    {
        var db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        AddCategory(db, 1, "Фурнітура", "furnitura", "n1", null);
        AddCategory(db, 2, "Замки", "zamky", "n1.n2", 1);
        AddCategory(db, 3, "Петлі", "petli", "n1.n3", 1);
        AddCategory(db, 4, "Старий розділ", "staryi-rozdil", "n1.n4", 1).MarkDeleted(Now);
        AddText(db, 1, "uk");
        AddText(db, 1, "ru");
        AddText(db, 2, "uk");

        AddProduct(db, 20, 2, photo: string.Empty, onSite: true);
        AddProduct(db, 21, 2, photo: string.Empty, onSite: true);
        AddProduct(db, 22, 2, photo: string.Empty, onSite: false);
        AddProduct(db, 23, 2, photo: "products/23.webp", onSite: true);
        AddProduct(db, 24, 3, photo: string.Empty, onSite: true);

        db.SaveChanges();
        return db;
    }

    private static ProductCategory AddCategory(CatalogDbContext db, int id, string name, string slug, string path, int? parentId)
    {
        var category = ProductCategory.Create(ProductCategoryId.From(id), "000005513", name, slug, CategoryPath.From(path),
            parentId is null ? null : ProductCategoryId.From(parentId.Value)).Value;
        category.SetShownOnSite(true, DateTimeOffset.UtcNow);
        db.Categories.Add(category);
        return category;
    }

    private static void AddText(CatalogDbContext db, int categoryId, string language)
    {
        var content = CategoryContent.Create(ProductCategoryId.From(categoryId), language);
        content.Replace(null, null, null, "<p>Текст</p>", [], "seed", Now);
        db.CategoryContents.Add(content);
    }

    private static void AddProduct(CatalogDbContext db, int id, int categoryId, string photo, bool onSite)
    {
        var slug = $"tovar-{id}";
        db.ProductListProjections.Add(ProductListProjection.Create(ProductId.From(id), slug, slug, slug, photo,
            ProductCategoryId.From(categoryId), "kategoriia", inStock: true, isSale: false, isNew: false,
            exportToSite: onSite, retailPrice: 100m));
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
