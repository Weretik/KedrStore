using Catalog.Application.Contracts.Media;
using Catalog.Application.Features.ProductMedia.FileChanges;
using Catalog.Application.Features.ProductMedia.Set;
using Catalog.Application.Jobs.ProductMedia;
using Catalog.Contracts.Products.Media;
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
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.DependencyInjection;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntegrationTests.Catalog.ProductMedia;

/// <summary>SC-30, SC-31, SC-33 and the list cache (R-33…R-35) on real PostgreSQL with a fake library.</summary>
[Collection(CatalogPostgresCollection.Name)]
public sealed class ProductMediaLinkPostgresTests(CatalogPostgresFixture postgres)
{
    private const string Root = "000005513";
    private static readonly DateTimeOffset Created = new(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);

    [SkippableFact]
    public async Task SyncRun_DryRunSavesNothing_ThenLinksByName_AndSecondRunLinksNothing()
    {
        postgres.SkipIfUnavailable();
        await SeedAsync();
        var library = new FakeProductMediaLibrary();
        var photo = library.Add(10, "products/731.jpg");
        var scheme = library.Add(11, "product-scheme/s731.JPG");
        library.Add(12, "products/733.jpg");
        library.Add(13, "products/old/732.jpg");
        library.Add(14, "products/readme.jpg");
        await using var provider = BuildProvider(library);

        var dry = await RunLinkAsync(provider, dryRun: true);
        Assert.Equal((1, 1, 0, 1), (dry.PhotosLinked, dry.SchemesLinked, dry.LinksDropped, dry.ProductsWithoutPhoto));
        await using (var db = postgres.CreateContext())
        {
            Assert.Equal(0, await db.Database.SqlQueryRaw<int>("""SELECT count(*)::int AS "Value" FROM "ProductMedia" """).SingleAsync());
            Assert.Equal("https://old.test/731.jpg", (await LoadProduct(db, 731)).Photo);
        }

        var applied = await RunLinkAsync(provider, dryRun: false);
        Assert.Equal((1, 1, 1), (applied.PhotosLinked, applied.SchemesLinked, applied.ProductsWithoutPhoto));
        await using (var db = postgres.CreateContext())
        {
            var linked = await LoadProduct(db, 731);
            Assert.Equal(photo.Url, linked.Photo);
            Assert.Equal(scheme.Url, linked.Sсheme);
            Assert.Equal(string.Empty, (await LoadProduct(db, 732)).Photo);
            var deleted = await LoadProduct(db, 733);
            Assert.Empty(deleted.MediaLinks);
            Assert.Equal(photo.Url, await ProjectionPhoto(db, 731));
            Assert.Equal(string.Empty, await ProjectionPhoto(db, 732));
        }

        var again = await RunLinkAsync(provider, dryRun: false);
        Assert.Equal((0, 0, 0), (again.PhotosLinked, again.SchemesLinked, again.ProductsChanged));

        library.Files.Remove(scheme.Id);
        var afterDelete = await RunLinkAsync(provider, dryRun: false);
        Assert.Equal(1, afterDelete.LinksDropped);
        await using (var db = postgres.CreateContext())
            Assert.Equal(string.Empty, (await LoadProduct(db, 731)).Sсheme);
    }

    [SkippableFact]
    public async Task SetMedia_ThenReplaceAndDeleteFile_UpdatesLinksCacheAndListRow()
    {
        postgres.SkipIfUnavailable();
        await SeedAsync();
        var library = new FakeProductMediaLibrary();
        var a = library.Add(20, "library/2026/11/a.jpg");
        var b = library.Add(21, "library/2026/11/b.jpg");
        var c = library.Add(22, "library/2026/11/c.png");
        await using var provider = BuildProvider(library);

        var set = await SendAsync(provider, new SetProductMediaCommand(731,
            new SetProductMediaRequest { PhotoIds = [b.Id, a.Id], SchemeId = c.Id }));
        Assert.True(set.IsSuccess);
        Assert.Equal([b.Id, a.Id], set.Value.Photos.Select(item => item.Id));
        Assert.Equal("b.jpg", set.Value.Photos[0].FileName);
        await using (var db = postgres.CreateContext())
            Assert.Equal(b.Url, await ProjectionPhoto(db, 731));

        var unknown = await SendAsync(provider, new SetProductMediaCommand(731,
            new SetProductMediaRequest { PhotoIds = [a.Id, 999] }));
        Assert.Equal(Ardalis.Result.ResultStatus.Invalid, unknown.Status);

        await using (var scope = provider.CreateAsyncScope())
        {
            var changes = scope.ServiceProvider.GetRequiredService<ProductMediaFileChanges>();
            await changes.ApplyReplacedAsync(b.Id, "https://images-kedr.cdn.express/library/2026/11/b2.jpg", default);
            await changes.ApplyDeletedAsync(b.Id, default);
        }

        await using (var db = postgres.CreateContext())
        {
            var product = await LoadProduct(db, 731);
            Assert.Equal([(a.Id, 0)], product.Photos.Select(link => (link.MediaFileId, link.Position)));
            Assert.Equal(a.Url, product.Photo);
            Assert.Equal(c.Url, product.Sсheme);
            Assert.Equal(a.Url, await ProjectionPhoto(db, 731));
        }
    }

    [SkippableFact]
    public async Task FolderRun_PlacesEachFileByTheCategoryOfItsFirstActiveProduct()
    {
        postgres.SkipIfUnavailable();
        await SeedAsync();
        await using (var db = postgres.CreateContext())
        {
            db.Categories.Add(ProductCategory.Create(ProductCategoryId.From(11), Root, "Петлі", "petli",
                CategoryPath.From("n10.n11"), ProductCategoryId.From(10)));
            db.Products.Add(Product.Create(ProductId.From(734), Root, "Петля 734", "petlia-734",
                ProductCategoryId.From(11), Created, 5, 1, false, false, true));
            await db.SaveChangesAsync();
        }

        var library = new FakeProductMediaLibrary();
        var shared = library.Add(30, "products/731.jpg");
        var own = library.Add(31, "products/734.jpg");
        var scheme = library.Add(32, "product-scheme/s734.jpg");
        await using var provider = BuildProvider(library);
        await RunLinkAsync(provider, dryRun: false);
        Assert.True((await SendAsync(provider, new SetProductMediaCommand(734,
            new SetProductMediaRequest { PhotoIds = [own.Id, shared.Id], SchemeId = scheme.Id }))).IsSuccess);

        await using (var scope = provider.CreateAsyncScope())
            await scope.ServiceProvider.GetRequiredService<ArrangeProductMediaFoldersJob>().RunAsync(dryRun: true, default);

        var photos = Assert.Single(library.Arranged, call => call.Directory == "products");
        Assert.True(photos.DryRun);
        Assert.Contains(new ProductMediaFolderNode(11, "Петлі", 10), photos.Categories);
        Assert.Contains(new ProductMediaFolderNode(10, "Завіси", null), photos.Categories);
        Assert.Equal(
            [new ProductMediaFolderPlacement(shared.Id, 10), new ProductMediaFolderPlacement(own.Id, 11)],
            photos.Placements.OrderBy(placement => placement.FileId));
        var schemes = Assert.Single(library.Arranged, call => call.Directory == "product-scheme");
        Assert.Equal([new ProductMediaFolderPlacement(scheme.Id, 11)], schemes.Placements);
    }

    private static async Task<LinkProductMediaReport> RunLinkAsync(ServiceProvider provider, bool dryRun)
    {
        await using var scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<LinkProductMediaJob>().RunAsync(dryRun, default);
    }

    private static async Task<Ardalis.Result.Result<ProductMediaDto>> SendAsync(ServiceProvider provider,
        SetProductMediaCommand command)
    {
        await using var scope = provider.CreateAsyncScope();
        var handler = ActivatorUtilities.CreateInstance<SetProductMediaCommandHandler>(scope.ServiceProvider);
        return await handler.Handle(command, default);
    }

    private static Task<Product> LoadProduct(CatalogDbContext db, int id)
        => db.Products.IgnoreQueryFilters().AsNoTracking().SingleAsync(product => product.Id == ProductId.From(id));

    private static Task<string> ProjectionPhoto(CatalogDbContext db, int id)
        => db.ProductListProjections.AsNoTracking()
            .Where(row => row.ProductId == ProductId.From(id)).Select(row => row.Photo).SingleAsync();

    private async Task SeedAsync()
    {
        await using var db = postgres.CreateContext();
        await db.Database.ExecuteSqlRawAsync(
            """TRUNCATE "ProductMedia", "ProductListProjections", "ProductPrices", "ProductTranslations", "Products", "CatalogSlugHistory", "CategoryFaqItems", "CategoryContents", "ProductCategories" CASCADE;""");
        db.Categories.Add(ProductCategory.Create(ProductCategoryId.From(10), Root, "Завіси", "zavisy", CategoryPath.From("n10")));
        foreach (var id in new[] { 731, 732, 733 })
        {
            var product = Product.Create(ProductId.From(id), Root, $"Завіса {id}",
                $"zavisa-{id}", ProductCategoryId.From(10), Created, 5, 1, false, false, true);
            if (id == 733)
                product.MarkDeleted(Created);
            db.Products.Add(product);
            if (id != 733)
            {
                db.ProductListProjections.Add(ProductListProjection.Create(product.Id, product.Name, product.Name,
                    product.ProductSlug, $"https://old.test/{id}.jpg", ProductCategoryId.From(10), "zavisy",
                    inStock: true, isSale: false, isNew: false, exportToSite: true, retailPrice: 100));
            }
        }

        await db.SaveChangesAsync();
        // Products imported before stage 3 still carry the URLs the 1C mapper built.
        await db.Database.ExecuteSqlRawAsync("""UPDATE "Products" SET "Photo" = 'https://old.test/' || "Id" || '.jpg'""");
    }

    private ServiceProvider BuildProvider(FakeProductMediaLibrary library)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Default"] = postgres.ConnectionString })
            .Build();
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddCatalogDbContextServices(configuration);
        services.AddProductMediaServices();
        services.RemoveAll<IProductMediaLibrary>();
        services.AddSingleton<IProductMediaLibrary>(library);
        return services.BuildServiceProvider();
    }
}
