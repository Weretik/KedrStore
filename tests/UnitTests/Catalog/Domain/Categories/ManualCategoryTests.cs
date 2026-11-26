using Catalog.Domain.Categories.Tree;
using BuildingBlocks.Domain.Identifiers;
using Catalog.Domain.Categories;

namespace UnitTests.Catalog.Domain.Categories;

/// <summary>Spec catalog/011: a category made by hand and its place in the tree.</summary>
public sealed class ManualCategoryTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateManual_UnderAParent_TakesItsPlaceVisibilityAndSearchClosing()
    {
        var parent = OneC(5513, "n1.n5513", "hardware");
        parent.SetShownOnSite(true, Now);
        parent.SetClosedFromSearch(true, Now);

        var created = ProductCategory.CreateManual(Id(800000001), " Накладні ручки ", "Накладные ручки",
            "nakladni-ruchky", parent, isShownOnSite: null, Now);

        var category = created.Value;
        Assert.True(category.IsManual);
        Assert.Equal("n1.n5513.n800000001", category.Path.Value);
        Assert.Equal(parent.Id, category.ParentId);
        Assert.Equal(2, category.Level);
        Assert.Equal("hardware", category.ProductTypeIdOneC);
        Assert.Equal("Накладні ручки", category.Name);
        Assert.Equal("Накладні ручки", category.ShortNameUk);
        Assert.Equal("Накладные ручки", category.ShortNameRu);
        Assert.Equal("Накладні ручки", category.SiteNameUk);
        Assert.Equal("Накладные ручки", category.SiteNameRu);
        Assert.True(category.IsShownOnSite);
        Assert.True(category.IsClosedFromSearch);
    }

    [Fact]
    public void CreateManual_AtTheTop_UsesTheManualRootAndTheGivenVisibility()
    {
        var category = ProductCategory.CreateManual(Id(800000002), "Акції", "Акции", "aktsii", null, false, Now).Value;

        Assert.Equal("n800000002", category.Path.Value);
        Assert.Null(category.ParentId);
        Assert.Equal(0, category.Level);
        Assert.Equal(ProductCategory.ManualRootKey, category.ProductTypeIdOneC);
        Assert.False(category.IsShownOnSite);
    }

    [Theory]
    [InlineData("", "Ручки")]
    [InlineData("Ручки", " ")]
    public void CreateManual_WithoutANameFails(string nameUk, string nameRu)
    {
        var created = ProductCategory.CreateManual(Id(800000003), nameUk, nameRu, "slug", null, null, Now);

        Assert.True(created.IsFailure);
    }

    [Fact]
    public void CreateManual_UnderADeletedParentFails()
    {
        var parent = OneC(5513, "n5513", "hardware");
        parent.MarkDeleted(Now);

        var created = ProductCategory.CreateManual(Id(800000004), "Ручки", "Ручки", "slug", parent, null, Now);

        Assert.Equal("Catalog.Category.Parent.Deleted", created.Error!.Code);
    }

    [Fact]
    public void MoveUnder_ChangesThePathAndRootAndGivesTheOldPath()
    {
        var category = Manual(800000005, OneC(5513, "n1.n5513", "hardware"));
        var target = OneC(7000, "n2.n7000", "doors");

        var moved = category.MoveUnder(target, Now);

        Assert.Equal("n1.n5513.n800000005", moved.Value!.Value.Value);
        Assert.Equal("n2.n7000.n800000005", category.Path.Value);
        Assert.Equal(target.Id, category.ParentId);
        Assert.Equal("doors", category.ProductTypeIdOneC);
        Assert.Equal(2, category.Level);
        Assert.Equal(Now, category.UpdatedAt);
    }

    [Fact]
    public void MoveUnder_TheSameParentChangesNothing()
    {
        var parent = OneC(5513, "n5513", "hardware");
        var category = Manual(800000006, parent);

        Assert.Null(category.MoveUnder(parent, Now).Value);
    }

    [Fact]
    public void MoveUnder_ItselfOrItsSubcategoryFails()
    {
        var category = Manual(800000007, null);
        var child = Manual(800000008, category);

        Assert.Equal("Catalog.Category.Move.IntoItself", category.MoveUnder(category, Now).Error!.Code);
        Assert.Equal("Catalog.Category.Move.IntoItself", category.MoveUnder(child, Now).Error!.Code);
    }

    [Fact]
    public void EnsureManual_RefusesAOneCCategory()
    {
        Assert.Equal("Catalog.Category.NotManual", OneC(5513, "n5513", "hardware").EnsureManual().Error!.Code);
    }

    [Fact]
    public void Rebuild_MovesSubcategoriesAlongWithTheirParent()
    {
        var parent = Manual(800000009, OneC(5513, "n1.n5513", "hardware"));
        var child = Manual(800000010, parent);
        parent.MoveUnder(null, Now);

        Assert.Equal(1, CategoryTreePlacement.Rebuild([child, parent], Now));
        Assert.Equal("n800000009.n800000010", child.Path.Value);
        Assert.Equal(1, child.Level);
        Assert.Equal(ProductCategory.ManualRootKey, child.ProductTypeIdOneC);
        Assert.Equal(0, CategoryTreePlacement.Rebuild([child, parent], Now));
    }
    private static ProductCategoryId Id(int value) => ProductCategoryId.Create(value);

    private static ProductCategory Manual(int id, ProductCategory? parent)
        => ProductCategory.CreateManual(Id(id), $"Категорія {id}", $"Категория {id}", $"c-{id}", parent, null, Now)
            .Value;

    private static ProductCategory OneC(int id, string path, string root)
    {
        var segments = path.Split('.');
        ProductCategoryId? parentId = segments.Length > 1
            ? ProductCategoryId.Create(int.Parse(segments[^2][1..]))
            : null;
        return ProductCategory.Create(Id(id), root, $"Категорія {id}", $"c-{id}", CategoryPath.Create(path), parentId)
            .Value;
    }
}
