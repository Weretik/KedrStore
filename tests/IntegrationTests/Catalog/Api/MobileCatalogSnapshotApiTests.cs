using System.Net;
using System.Net.Http.Headers;
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
using NodaMoney;

namespace IntegrationTests.Catalog.Api;

/// <summary>Catalog snapshot for the offline manager app (spec platform/004, TS-001) on an in-memory catalog.</summary>
public sealed class MobileCatalogSnapshotApiTests
{
    private const string Route = "/api/mobile/sync/catalog";
    private const string Hardware = "000005513";
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Snapshot_HasActiveProductsWithPricesPhotosAndCategories()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var response = await manager.GetAsync(Route);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;
        Assert.Equal(response.Headers.ETag!.Tag, $"\"{root.GetProperty("version").GetString()}\"");
        Assert.Equal(2, root.GetProperty("priceTypes").GetArrayLength());
        Assert.Equal([1, 2], root.GetProperty("categories").EnumerateArray().Select(c => c.GetProperty("id").GetInt32()));
        Assert.Equal(1, root.GetProperty("categories")[1].GetProperty("parentId").GetInt32());

        var products = root.GetProperty("products");
        Assert.Equal([20, 21], products.EnumerateArray().Select(p => p.GetProperty("id").GetInt32()));
        var lockProduct = products[0];
        Assert.Equal("Замок український", lockProduct.GetProperty("name").GetString());
        Assert.Equal(["https://cdn/20-a.webp", "https://cdn/20-b.webp"],
            lockProduct.GetProperty("photos").EnumerateArray().Select(p => p.GetString()));
        Assert.Equal("https://cdn/20-scheme.webp", lockProduct.GetProperty("scheme").GetString());
        Assert.True(lockProduct.GetProperty("inStock").GetBoolean());
        var prices = lockProduct.GetProperty("prices");
        Assert.Equal(2, prices.GetArrayLength());
        Assert.Equal(12, prices[0].GetProperty("priceTypeId").GetInt32());
        Assert.Equal(150m, prices[0].GetProperty("amount").GetDecimal());

        var lowStock = products[1];
        Assert.False(lowStock.GetProperty("inStock").GetBoolean());
        Assert.Equal(JsonValueKind.Null, lowStock.GetProperty("scheme").ValueKind);
        Assert.Empty(lowStock.GetProperty("photos").EnumerateArray());
    }

    [Fact]
    public async Task Snapshot_AnswersNotModifiedForTheSameVersion_AndANewVersionAfterAPriceChange()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");

        using var first = await manager.GetAsync(Route);
        var etag = first.Headers.ETag!;

        using var again = new HttpRequestMessage(HttpMethod.Get, Route);
        again.Headers.IfNoneMatch.Add(etag);
        using var notModified = await manager.SendAsync(again);
        Assert.Equal(HttpStatusCode.NotModified, notModified.StatusCode);
        Assert.Empty(await notModified.Content.ReadAsByteArrayAsync());

        var price = db.ProductPrices.Single(p => p.ProductId == ProductId.From(20) && p.PriceTypeId == PriceTypeId.From(12));
        price.Update(ProductId.From(20), PriceTypeId.From(12), new Money(175m, "UAH"));
        db.SaveChanges();

        using var changed = new HttpRequestMessage(HttpMethod.Get, Route);
        changed.Headers.IfNoneMatch.Add(etag);
        using var updated = await manager.SendAsync(changed);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.NotEqual(etag, updated.Headers.ETag);
    }

    [Fact]
    public async Task Snapshot_IsCompressedWhenTheClientAcceptsGzip()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var manager = factory.CreateClientWithRole("Manager");
        using var request = new HttpRequestMessage(HttpMethod.Get, Route);
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

        using var response = await manager.SendAsync(request);

        Assert.Contains("gzip", response.Content.Headers.ContentEncoding);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task Snapshot_IsForManagersAndAdmins(string? role, HttpStatusCode expected)
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        using var response = await client.GetAsync(Route);

        Assert.Equal(expected, response.StatusCode);
    }

    private static CatalogDbContext Seed()
    {
        var db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        db.PriceTypes.Add(PriceType.Create(PriceTypeId.From(12), "Роздрібна"));
        db.PriceTypes.Add(PriceType.Create(PriceTypeId.From(14), "Оптова"));
        AddCategory(db, 1, "Фурнітура", "n1", null);
        AddCategory(db, 2, "Замки", "n1.n2", 1);
        AddCategory(db, 3, "Старий розділ", "n1.n3", 1).MarkDeleted(Now);

        var lockProduct = AddProduct(db, 20, 2, stock: 5);
        lockProduct.SetMedia(
            [new ProductMediaFile(1, "https://cdn/20-a.webp"), new ProductMediaFile(2, "https://cdn/20-b.webp")],
            new ProductMediaFile(3, "https://cdn/20-scheme.webp"), Now);
        db.ProductTranslations.Add(ProductTranslation.Create(ProductId.From(20), "uk", "Замок український", Now));
        db.ProductTranslations.Add(ProductTranslation.Create(ProductId.From(20), "ru", "Замок русский", Now));
        AddPrice(db, 20, 12, 150m);
        AddPrice(db, 20, 14, 120m);

        AddProduct(db, 21, 2, stock: 2);
        AddPrice(db, 21, 12, 80m);

        AddProduct(db, 22, 2, stock: 9).MarkDeleted(Now);
        AddPrice(db, 22, 12, 99m);

        db.SaveChanges();
        return db;
    }

    private static ProductCategory AddCategory(CatalogDbContext db, int id, string name, string path, int? parentId)
    {
        var category = ProductCategory.Create(ProductCategoryId.From(id), Hardware, name, $"category-{id}",
            CategoryPath.From(path), parentId is null ? null : ProductCategoryId.From(parentId.Value));
        db.Categories.Add(category);
        return category;
    }

    private static Product AddProduct(CatalogDbContext db, int id, int categoryId, decimal stock)
    {
        var product = Product.Create(ProductId.From(id), Hardware, $"Товар {id}", $"tovar-{id}",
            ProductCategoryId.From(categoryId), Now, stock, qtyInPack: 1, isNew: false, isSale: false);
        db.Products.Add(product);
        return product;
    }

    private static void AddPrice(CatalogDbContext db, int productId, int priceTypeId, decimal amount)
        => db.ProductPrices.Add(ProductPrice.Create(Hardware, ProductId.From(productId), PriceTypeId.From(priceTypeId),
            new Money(amount, "UAH")));

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
