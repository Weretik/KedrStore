using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Domain.Identifiers;
using Catalog.Application.Abstractions.Media;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
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
/// Spec catalog/012 over HTTP on an in-memory catalog: a category made by hand, moving any category, the order by hand,
/// and deleting an empty manual category.
/// </summary>
public sealed class CategoryPlacementApiTests
{
    [Fact]
    public async Task Admin_CreatesACategoryUnderAParent_AndTheSameNameAgainIsAConflict_SCMC01()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var admin = factory.CreateClientWithRole("Admin");

        using var created = await admin.PostAsJsonAsync("/api/categories",
            new { nameUk = "Накладні замки", nameRu = "Накладные замки", parentId = 2 });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var body = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        Assert.Equal(800_000_000, body.RootElement.GetProperty("id").GetInt32());
        Assert.True(body.RootElement.GetProperty("isManual").GetBoolean());
        Assert.Equal(2, body.RootElement.GetProperty("parentId").GetInt32());
        Assert.True(body.RootElement.GetProperty("isShownOnSite").GetBoolean());
        Assert.Equal("Накладные замки", body.RootElement.GetProperty("shortNameRu").GetString());
        var saved = await db.Categories.AsNoTracking().SingleAsync(category => category.Id.Value == 800_000_000);
        Assert.Equal("n1.n2.n800000000", saved.Path.Value);

        using var again = await admin.PostAsJsonAsync("/api/categories",
            new { nameUk = "накладні замки", nameRu = "Накладные", parentId = 2 });
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
    }

    [Fact]
    public async Task Admin_CannotCreateUnderACategoryWithModels()
    {
        await using var db = Seed();
        db.ProductGroups.Add(ProductGroup.CreateManual(ProductCategoryId.Create(3), "Замок Рим", "zamok-rym",
            DateTimeOffset.UtcNow).Value);
        await db.SaveChangesAsync();
        using var factory = CreateFactory(db);
        using var admin = factory.CreateClientWithRole("Admin");

        using var created = await admin.PostAsJsonAsync("/api/categories",
            new { nameUk = "Нова", nameRu = "Новая", parentId = 3 });

        Assert.Equal(HttpStatusCode.BadRequest, created.StatusCode);
    }

    [Fact]
    public async Task Admin_MovesAOneCCategoryWithItsSubcategory_AndBack_SCMC02()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var admin = factory.CreateClientWithRole("Admin");

        using var moved = await admin.PutAsJsonAsync("/api/categories/2/parent", new { parentId = 4 });

        Assert.Equal(HttpStatusCode.OK, moved.StatusCode);
        Assert.Equal("n4.n2.n3", await PathOf(db, 3));
        Assert.True((await db.Categories.AsNoTracking().SingleAsync(category => category.Id.Value == 2)).IsPlacedByHand);

        using var intoChild = await admin.PutAsJsonAsync("/api/categories/2/parent", new { parentId = 3 });
        Assert.Equal(HttpStatusCode.BadRequest, intoChild.StatusCode);

        using var back = await admin.PutAsJsonAsync("/api/categories/2/parent", new { parentId = 1 });
        Assert.Equal(HttpStatusCode.OK, back.StatusCode);
        Assert.Equal("n1.n2.n3", await PathOf(db, 3));
        Assert.False((await db.Categories.AsNoTracking().SingleAsync(category => category.Id.Value == 2)).IsPlacedByHand);
    }

    [Fact]
    public async Task Admin_MovesACategoryDown_AndTheOrderIsByHand_SCMC06()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var admin = factory.CreateClientWithRole("Admin");

        using var down = await admin.PutAsJsonAsync("/api/categories/1/order", new { direction = "down" });

        Assert.Equal(HttpStatusCode.OK, down.StatusCode);
        var roots = await db.Categories.AsNoTracking().Where(category => category.ParentId == null)
            .OrderBy(category => category.SortOrder).Select(category => category.Id.Value).ToListAsync();
        Assert.Equal([4, 1], roots);

        using var bad = await admin.PutAsJsonAsync("/api/categories/1/order", new { direction = "left" });
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
    }

    [Fact]
    public async Task Admin_DeletesOnlyAnEmptyManualCategory_SCMC04()
    {
        await using var db = Seed();
        using var factory = CreateFactory(db);
        using var admin = factory.CreateClientWithRole("Admin");
        using var created = await admin.PostAsJsonAsync("/api/categories", new { nameUk = "Акції", nameRu = "Акции" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var child = await admin.PostAsJsonAsync("/api/categories",
            new { nameUk = "Знижки", nameRu = "Скидки", parentId = 800_000_000 });
        Assert.Equal(HttpStatusCode.Created, child.StatusCode);

        using var oneC = await admin.DeleteAsync("/api/categories/2");
        Assert.Equal(HttpStatusCode.BadRequest, oneC.StatusCode);
        using var withChild = await admin.DeleteAsync("/api/categories/800000000");
        Assert.Equal(HttpStatusCode.Conflict, withChild.StatusCode);

        using var deleted = await admin.DeleteAsync("/api/categories/800000001");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.True((await db.Categories.IgnoreQueryFilters().AsNoTracking()
            .SingleAsync(category => category.Id.Value == 800_000_001)).IsDeleted);
    }

    private static Task<string> PathOf(CatalogDbContext db, int id)
        => db.Categories.AsNoTracking().Where(category => category.Id.Value == id)
            .Select(category => category.Path.Value).SingleAsync();

    private static CatalogDbContext Seed()
    {
        var db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        AddCategory(db, 1, "Фурнітура", "furnitura", "n1", null, 0);
        AddCategory(db, 2, "Замки", "zamky", "n1.n2", 1, 0);
        AddCategory(db, 3, "Врізні", "vrizni", "n1.n2.n3", 2, 0);
        AddCategory(db, 4, "Двері", "dveri", "n4", null, 1);
        db.SaveChanges();
        return db;
    }

    private static void AddCategory(CatalogDbContext db, int id, string name, string slug, string path, int? parentId,
        int sortOrder)
    {
        var category = ProductCategory.Create(ProductCategoryId.Create(id), "000005513", name, slug,
            CategoryPath.Create(path), parentId is null ? null : ProductCategoryId.Create(parentId.Value)).Value;
        category.UpdatePresentationMetadata(name, name, sortOrder, category.Level);
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
                services.RemoveAll<IManualCategoryIds>();
                services.AddSingleton<IManualCategoryIds>(new CountingManualCategoryIds());
            });
        });

    private sealed class CountingManualCategoryIds : IManualCategoryIds
    {
        private int _next = 800_000_000;

        public Task<ProductCategoryId> NextAsync(CancellationToken cancellationToken)
            => Task.FromResult(ProductCategoryId.Create(_next++));
    }
}
