using BuildingBlocks.Domain.Identifiers;
using Catalog.Application.Features.Categories.Shared;
using Catalog.Application.Features.Categories.Shared;
using Catalog.Application.Features.OneCSync.Source;
using Catalog.Application.Features.OneCSync.Mapping;
using Catalog.Application.Features.OneCSync.Sync;
using Catalog.Application.Features.OneCSync.Sync;
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
using Catalog.Application.Features.Categories.Shared.Stats;
using Catalog.Application.Features.OneCSync.Sync.Categories;

namespace UnitTests.Catalog.Sync;

public sealed class ImportSyncTests
{
    [Theory]
    [InlineData(100, 0, true)]
    [InlineData(100, 30, true)]
    [InlineData(100, 31, false)]
    [InlineData(100, 40, false)]
    [InlineData(0, 0, true)]
    [InlineData(3, 1, false)]
    public void MassDeletionGuard_BlocksMoreThanThirtyPercent(int active, int toDelete, bool allowed)
        => Assert.Equal(allowed, MassDeletionGuard.Allows(active, toDelete));

    [Fact]
    public void CategoryNameMap_ResolvesTrimmedCaseInsensitiveUniqueNames()
    {
        var map = new CategoryNameMap([new CategoryNameRow(10, "Замки"), new CategoryNameRow(11, "Ручки"), new CategoryNameRow(12, "ручки ")]);

        Assert.Equal(10, map.Resolve("  замки "));
        Assert.Null(map.Resolve("Ручки"));
        Assert.Null(map.Resolve("Невідома"));
        Assert.Equal(["Ручки"], map.AmbiguousNames);
        Assert.Equal(["Невідома"], map.MissingNames);
    }

    [Fact]
    public void CategoryNameMap_ResolvesBy1CFolderPathThenName_IgnoringOurOwnGroups()
    {
        // Our database also holds the configured group «Ручки» and a manual category: 1C does not know them.
        var active = new[]
        {
            new CategoryNameRow(5915, "На планці"), new CategoryNameRow(5853, "R-08/R-10"),
            new CategoryNameRow(7001, "На планці"), new CategoryNameRow(900003, "Ручки"),
            new CategoryNameRow(800000001, "Ручки")
        };
        var oneC = new[]
        {
            new OneCCategoryDto(5900, 5513, "Ручки", "Фурнітура/Ручки"),
            new OneCCategoryDto(5915, 5900, "На планці", "Фурнітура/Ручки/На планці"),
            new OneCCategoryDto(5853, 5900, "R-08/R-10", "Фурнітура/Ручки/R-08/R-10"),
            new OneCCategoryDto(7001, 7000, "На планці", "Фурнітура/Засувки/На планці")
        };

        var map = new CategoryNameMap(active, oneC);

        Assert.Equal(5915, map.Resolve(" фурнітура / ручки / на планці "));
        Assert.Equal(5853, map.Resolve("R-08/R-10"));
        Assert.Null(map.Resolve("Ручки"));
        Assert.Equal(["Ручки"], map.MissingNames);
        Assert.Null(map.Resolve("На планці"));
        Assert.Equal(["На планці"], map.AmbiguousNames);
    }

    [Fact]
    public void CategoryNameMap_WithoutTheFolderListFallsBackToOurNames()
    {
        var map = new CategoryNameMap([new CategoryNameRow(10, "Замки")], []);

        Assert.Equal(10, map.Resolve("Замки"));
    }

    [Fact]
    public void MapProduct_LeavesCategoryEmptyWhenNameDoesNotResolve()
    {
        var map = new CategoryNameMap([new CategoryNameRow(10, "Замки")]);
        var source = new[]
        {
            new OneCProductDto(1, " Замок ", "Замки", "", false, false, true, 1),
            new OneCProductDto(2, "Ручка", "Ручки", "", false, false, true, 1)
        };

        var mapped = CatalogMapper.MapProduct(source, map, "R");

        Assert.Equal(10, mapped[0].CategoryId);
        Assert.Equal("Замок", mapped[0].Name);
        Assert.Null(mapped[1].CategoryId);
    }

    [Fact]
    public void CategoryStatsRollup_AddsDescendantsToAncestors()
    {
        var categories = new[]
        {
            Category(1, "n1"), Category(2, "n1.n2"), Category(3, "n1.n2.n3"), Category(4, "n1.n4")
        };
        var direct = new Dictionary<int, CategoryStats>
        {
            [2] = new(2, 100m, 300m),
            [3] = new(1, 50m, 50m),
            [4] = new(0, null, null)
        };

        var rolled = CategoryStatsRollup.Rollup(categories, direct);

        Assert.Equal(new CategoryStats(3, 50m, 300m), rolled[1]);
        Assert.Equal(new CategoryStats(3, 50m, 300m), rolled[2]);
        Assert.Equal(new CategoryStats(1, 50m, 50m), rolled[3]);
        Assert.Equal(0, rolled[4].ProductCount);
    }

    private static ProductCategory Category(int id, string path)
        => ProductCategory.Create(ProductCategoryId.Create(id), "R", $"Category {id}", $"category-{id}", CategoryPath.Create(path)).Value;
}
