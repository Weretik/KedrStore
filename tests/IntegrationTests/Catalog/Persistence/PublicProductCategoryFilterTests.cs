using Ardalis.Result;
using Catalog.Application.Features.Products.Queries.GetPublicProductList;
using Catalog.Application.Features.Slugs.Shared;
using Catalog.Contracts.Products.GetList;
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
using Catalog.Application.Features.OneCSync.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IntegrationTests.Catalog.Persistence;

public sealed class PublicProductCategoryFilterTests
{
    [Fact]
    public async Task ParentSlug_IncludesEveryDescendantBeforeCountAndPaging()
    {
        await using var db = CreateContext();
        SeedCategory(db, 1, "parent");
        SeedCategory(db, 2, "child", 1);
        SeedCategory(db, 3, "grandchild", 2);
        SeedCategory(db, 4, "sibling");
        SeedProduct(db, 11, 1, "parent");
        SeedProduct(db, 12, 2, "child");
        SeedProduct(db, 13, 3, "grandchild");
        SeedProduct(db, 14, 4, "sibling");
        SeedProduct(db, 15, 3, "grandchild", exportToSite: false);
        await db.SaveChangesAsync();

        var handler = new GetPublicProductListHandler(db, new SlugService(db), Roots);
        var first = await handler.Handle(new GetPublicProductListQuery(new GetProductsRequest
        {
            CategorySlug = "parent", Page = 1, PageSize = 2
        }), CancellationToken.None);
        var second = await handler.Handle(new GetPublicProductListQuery(new GetProductsRequest
        {
            CategorySlug = "parent", Page = 2, PageSize = 2
        }), CancellationToken.None);
        var byId = await handler.Handle(new GetPublicProductListQuery(new GetProductsRequest
        {
            CategoryId = 1, CategorySlug = "sibling", PageSize = 10
        }), CancellationToken.None);
        var leaf = await handler.Handle(new GetPublicProductListQuery(new GetProductsRequest
        {
            CategoryId = 3
        }), CancellationToken.None);
        var unknown = await handler.Handle(new GetPublicProductListQuery(new GetProductsRequest
        {
            CategorySlug = "unknown"
        }), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(3, first.Value.PagedInfo.TotalRecords);
        Assert.Equal(2, first.Value.PagedInfo.TotalPages);
        Assert.Equal(3, second.Value.PagedInfo.TotalRecords);
        Assert.Equal([11, 12, 13], first.Value.Value.Concat(second.Value.Value).Select(row => row.Id));
        Assert.Equal([11, 12, 13], byId.Value.Value.Select(row => row.Id));
        Assert.Equal([13], leaf.Value.Value.Select(row => row.Id));
        Assert.Equal(ResultStatus.NotFound, unknown.Status);
    }

    [Fact]
    public async Task CosmosProducts_AreNeverListed()
    {
        await using var db = CreateContext();
        SeedCategory(db, 1, "parent");
        // Even marked «show on site», the Cosmos import root stays hidden.
        var cosmos = ProductCategory.Create(
            ProductCategoryId.From(920000), "Космос", "Космос", "kosmos", CategoryPath.From("n920000")).Value;
        cosmos.SetShownOnSite(true, DateTimeOffset.UtcNow);
        db.Categories.Add(cosmos);
        SeedProduct(db, 11, 1, "parent");
        SeedProduct(db, 12, 920000, "kosmos");
        await db.SaveChangesAsync();

        var handler = new GetPublicProductListHandler(db, new SlugService(db), Roots);
        var all = await handler.Handle(new GetPublicProductListQuery(new GetProductsRequest { PageSize = 10 }),
            CancellationToken.None);

        Assert.Equal([11], all.Value.Value.Select(row => row.Id));
        Assert.Equal(1, all.Value.PagedInfo.TotalRecords);
    }

    private static readonly IOptions<RootCategoryId> Roots = Options.Create(new RootCategoryId
    {
        DoorsRootCategoryId = "000007226",
        HardwareRootCategoryId = "000005513",
        CosmosRootCategoryId = "Космос",
        CosmosCategoryId = 920000
    });

    private static CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CatalogDbContext(options);
    }

    private static void SeedCategory(CatalogDbContext db, int id, string slug, int? parentId = null)
    {
        var path = parentId switch
        {
            null => CategoryPath.Root(id.ToString()),
            1 => CategoryPath.From($"n1.n{id}"),
            2 => CategoryPath.From($"n1.n2.n{id}"),
            _ => throw new ArgumentOutOfRangeException(nameof(parentId))
        };
        var category = ProductCategory.Create(
            ProductCategoryId.From(id), "000005513", $"Category {id}", slug, path,
            parentId is null ? null : ProductCategoryId.From(parentId.Value)).Value;
        category.SetShownOnSite(true, DateTimeOffset.UtcNow);
        db.Categories.Add(category);
    }

    private static void SeedProduct(
        CatalogDbContext db, int id, int categoryId, string categorySlug, bool exportToSite = true)
    {
        db.ProductListProjections.Add(ProductListProjection.Create(
            ProductId.From(id), $"Product {id}", $"Product {id}", $"product-{id}",
            $"https://example.test/{id}.jpg", ProductCategoryId.From(categoryId), categorySlug,
            inStock: true, isSale: false, isNew: false, exportToSite, retailPrice: 10));
    }
}
