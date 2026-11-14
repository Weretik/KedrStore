using Catalog.Domain.Categories;

namespace UnitTests.Catalog.Domain;

public sealed class CategorySiteVisibilityTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 14, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Show_ShowsTheBranchAndEveryActiveParent()
    {
        var root = Category(1, "n1");
        var middle = Category(2, "n1.n2", 1);
        var selected = Category(3, "n1.n2.n3", 2);
        var child = Category(4, "n1.n2.n3.n4", 3);
        var sibling = Category(5, "n1.n5", 1);

        var changed = CategorySiteVisibility.Apply(selected, [root, middle, selected, child, sibling], true, Now);

        Assert.Equal(4, changed);
        Assert.True(root.IsShownOnSite);
        Assert.True(middle.IsShownOnSite);
        Assert.True(selected.IsShownOnSite);
        Assert.True(child.IsShownOnSite);
        Assert.False(sibling.IsShownOnSite);
    }

    [Fact]
    public void Hide_HidesTheBranchIncludingDeletedSubcategoriesButKeepsParents()
    {
        var root = Category(1, "n1");
        var selected = Category(2, "n1.n2", 1);
        var deletedChild = Category(3, "n1.n2.n3", 2);
        foreach (var category in new[] { root, selected, deletedChild })
            category.SetShownOnSite(true, Now);
        deletedChild.MarkDeleted(Now);

        var changed = CategorySiteVisibility.Apply(selected, [root, selected, deletedChild], false, Now);

        Assert.Equal(2, changed);
        Assert.True(root.IsShownOnSite);
        Assert.False(selected.IsShownOnSite);
        Assert.False(deletedChild.IsShownOnSite);
    }

    [Fact]
    public void Show_SkipsDeletedParentsAndDoesNotMatchPathPrefixesOfOtherNodes()
    {
        var deletedRoot = Category(1, "n1");
        deletedRoot.MarkDeleted(Now);
        var selected = Category(2, "n1.n2", 1);
        var lookalike = Category(22, "n1.n22", 1);

        var changed = CategorySiteVisibility.Apply(selected, [deletedRoot, selected, lookalike], true, Now);

        Assert.Equal(1, changed);
        Assert.False(deletedRoot.IsShownOnSite);
        Assert.False(lookalike.IsShownOnSite);
    }

    [Fact]
    public void Apply_ReturnsZeroWhenNothingChanges()
    {
        var selected = Category(1, "n1");

        Assert.Equal(0, CategorySiteVisibility.Apply(selected, [selected], false, Now));
    }

    private static ProductCategory Category(int id, string path, int? parentId = null)
        => ProductCategory.Create(ProductCategoryId.From(id), $"{id:D9}", $"Категорія {id}", $"category-{id}",
            CategoryPath.From(path), parentId is { } parent ? ProductCategoryId.From(parent) : null);
}
