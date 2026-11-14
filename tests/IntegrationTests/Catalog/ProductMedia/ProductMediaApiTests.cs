using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Catalog.Application.Abstractions.Persistence;
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
using IntegrationTests.Identity.Avatars;
using IntegrationTests.Media;
using IntegrationTests.TestSupport.Api;
using Media.Application.Contracts.Persistence;
using Media.Application.Contracts.Storage;
using Media.Infrastructure.DataBase;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests.Catalog.ProductMedia;

/// <summary>SC-32, SC-33, SC-35 over HTTP, on in-memory Catalog and Media stores and a fake CDN.</summary>
public sealed class ProductMediaApiTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.OK)]
    public async Task ProductMedia_IsForManagersAndAdmins(string? role, HttpStatusCode expected)
    {
        await using var catalog = SeedCatalog();
        using var factory = CreateFactory(catalog, new FakeMediaStorage());
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        Assert.Equal(expected, (await client.GetAsync("/api/admin/products/731/media")).StatusCode);
        using var put = await client.PutAsJsonAsync("/api/admin/products/731/media", new { photoIds = Array.Empty<long>() });
        Assert.Equal(expected, put.StatusCode);
    }

    [Fact]
    public async Task Manager_SetsOrderedPhotosAndScheme_ThenReplaceAndDeleteFollowTheLibrary()
    {
        await using var catalog = SeedCatalog();
        using var factory = CreateFactory(catalog, new FakeMediaStorage());
        using var manager = factory.CreateClientWithRole("Manager");
        using var admin = factory.CreateClientWithRole("Admin");
        var a = await UploadAsync(manager, "a.jpg");
        var b = await UploadAsync(manager, "b.jpg");
        var c = await UploadAsync(manager, "scheme.jpg");

        using var set = await manager.PutAsJsonAsync("/api/admin/products/731/media", new { photoIds = new[] { b.Id, a.Id }, schemeId = c.Id });
        Assert.Equal(HttpStatusCode.OK, set.StatusCode);
        using (var media = JsonDocument.Parse(await manager.GetStringAsync("/api/admin/products/731/media")))
        {
            var photos = media.RootElement.GetProperty("photos").EnumerateArray().ToArray();
            Assert.Equal([b.Id, a.Id], photos.Select(photo => photo.GetProperty("id").GetInt64()));
            Assert.Equal("b.jpg", photos[0].GetProperty("fileName").GetString());
            Assert.Equal(c.Url, media.RootElement.GetProperty("scheme").GetProperty("url").GetString());
        }

        using (var details = JsonDocument.Parse(await manager.GetStringAsync("/api/catalog/uk/product/zavisa-731")))
        {
            Assert.Equal(b.Url, details.RootElement.GetProperty("photo").GetString());
            Assert.Equal(c.Url, details.RootElement.GetProperty("scheme").GetString());
            Assert.Equal([b.Url, a.Url], details.RootElement.GetProperty("photos").EnumerateArray().Select(url => url.GetString()));
        }

        using var unknown = await manager.PutAsJsonAsync("/api/admin/products/731/media", new { photoIds = new[] { a.Id, 99_999L } });
        Assert.Equal(HttpStatusCode.BadRequest, unknown.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await manager.PutAsJsonAsync("/api/admin/products/1/media", new { photoIds = Array.Empty<long>() })).StatusCode);

        using var replace = await manager.PutAsync($"/api/admin/media/files/{b.Id}/content", Form(TestImages.Create(20, 20), "b2.jpg"));
        var newUrl = JsonDocument.Parse(await replace.Content.ReadAsStringAsync()).RootElement.GetProperty("url").GetString();
        Assert.NotEqual(b.Url, newUrl);
        Assert.Equal(newUrl, (await ReloadAsync(catalog)).Photo);

        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/admin/media/files/{b.Id}")).StatusCode);
        var product = await ReloadAsync(catalog);
        Assert.Equal([a.Id], product.Photos.Select(link => link.MediaFileId));
        Assert.Equal(a.Url, product.Photo);
    }

    [Fact]
    public async Task AdminList_WithoutPhoto_ListsOnlyProductsWithoutAPhoto()
    {
        await using var catalog = SeedCatalog();
        using var factory = CreateFactory(catalog, new FakeMediaStorage());
        using var manager = factory.CreateClientWithRole("Manager");

        using var list = JsonDocument.Parse(await manager.GetStringAsync("/api/admin/products?withoutPhoto=true"));

        var ids = list.RootElement.GetProperty("value").EnumerateArray().Select(row => row.GetProperty("id").GetInt32());
        Assert.Equal([732], ids);
    }

    private static async Task<Product> ReloadAsync(CatalogDbContext catalog)
    {
        catalog.ChangeTracker.Clear();
        return await catalog.Products.SingleAsync(product => product.Id == ProductId.From(731));
    }

    private static async Task<(long Id, string Url)> UploadAsync(HttpClient client, string fileName)
    {
        using var response = await client.PostAsync("/api/admin/media/files", Form(TestImages.Create(10, 10), fileName));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return (json.RootElement.GetProperty("id").GetInt64(), json.RootElement.GetProperty("url").GetString()!);
    }

    private static MultipartFormDataContent Form(byte[] bytes, string fileName)
    {
        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(file, "file", fileName);
        return form;
    }

    private static CatalogDbContext SeedCatalog()
    {
        var db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.Categories.Add(ProductCategory.Create(ProductCategoryId.From(10), "000005513", "Завіси", "zavisy",
            CategoryPath.From("n10")));
        foreach (var (id, photo) in new[] { (731, "https://images-kedr.cdn.express/products/731.jpg"), (732, "") })
        {
            var product = Product.Create(ProductId.From(id), "000005513", $"Завіса {id}", $"zavisa-{id}",
                ProductCategoryId.From(10), Now, 5, 1, false, false, true);
            db.Products.Add(product);
            db.ProductListProjections.Add(ProductListProjection.Create(product.Id, product.Name, product.Name,
                product.ProductSlug, photo, ProductCategoryId.From(10), "zavisy", inStock: true, isSale: false,
                isNew: false, exportToSite: true, retailPrice: 100));
        }

        db.SaveChanges();
        return db;
    }

    private static WebApplicationFactory<Program> CreateFactory(CatalogDbContext catalog, FakeMediaStorage cdn)
    {
        var media = new MediaDbContext(new DbContextOptionsBuilder<MediaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.AddTestRoleAuthentication();
                services.RemoveAll<IReadCatalogDbContext>();
                services.RemoveAll<CatalogDbContext>();
                services.AddSingleton(catalog);
                services.AddSingleton<IReadCatalogDbContext>(catalog);
                // The in-memory store has no SQL; the PostgreSQL tests cover the list row refresh.
                services.RemoveAll<IProductListProjectionRebuilder>();
                services.AddSingleton<IProductListProjectionRebuilder, NoopProjections>();
                services.RemoveAll<IReadMediaDbContext>();
                services.RemoveAll<MediaDbContext>();
                services.AddSingleton(media);
                services.AddSingleton<IReadMediaDbContext>(media);
                services.RemoveAll<IMediaStorage>();
                services.AddSingleton<IMediaStorage>(cdn);
            });
        });
    }

    private sealed class NoopProjections : IProductListProjectionRebuilder
    {
        public Task RebuildAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<int> RefreshPhotosAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }
}
