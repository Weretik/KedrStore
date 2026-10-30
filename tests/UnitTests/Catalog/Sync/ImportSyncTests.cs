using Catalog.Application.Features.Category.Stats;
using Catalog.Application.Features.Category;
using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Integrations.OneC.Mappers;
using Catalog.Application.Integrations.OneC.Specifications;
using Catalog.Application.Integrations.OneC.Sync;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

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
        => ProductCategory.Create(ProductCategoryId.From(id), "R", $"Category {id}", $"category-{id}", CategoryPath.From(path));
}
