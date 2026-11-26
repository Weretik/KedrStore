using Catalog.Domain.Categories.Tree;
using BuildingBlocks.Domain.Identifiers;
using Catalog.Domain.Categories;

namespace UnitTests.Catalog.Domain.Categories;

/// <summary>Spec catalog/011 MC-09, MC-10: an admin's place and order of a 1C category win over 1C.</summary>
public sealed class CategoryPlacementTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    public void MoveUnder_PutsAOneCCategoryElsewhereAndKeepsItsRoot()
    {
        var category = OneC(5513, "n1.n5513", "hardware");
        var target = OneC(7000, "n2.n7000", "doors");

        var moved = category.MoveUnder(target, Now);

        Assert.Equal("n1.n5513", moved.Value!.Value.Value);
        Assert.Equal("n2.n7000.n5513", category.Path.Value);
        Assert.True(category.IsPlacedByHand);
        Assert.Equal("hardware", category.ProductTypeIdOneC);
        Assert.Equal(ProductCategoryId.Create(1), category.OneCParentId);
    }

    [Fact]
    public void MoveUnder_BackUnderTheOneCParentIsNoLongerByHand()
    {
        var oneCParent = OneC(1, "n1", "hardware");
        var category = OneC(5513, "n1.n5513", "hardware");
        category.MoveUnder(null, Now);

        category.MoveUnder(oneCParent, Now);

        Assert.False(category.IsPlacedByHand);
        Assert.Equal("n1.n5513", category.Path.Value);
    }

    [Fact]
    public void MoveUnder_ItselfOrItsSubcategoryFails()
    {
        var category = OneC(5513, "n1.n5513", "hardware");
        var child = OneC(6000, "n1.n5513.n6000", "hardware");

        Assert.Equal("Catalog.Category.Move.IntoItself", category.MoveUnder(child, Now).Error!.Code);
        Assert.Equal("Catalog.Category.Move.IntoItself", category.MoveUnder(category, Now).Error!.Code);
    }

    [Fact]
    public void UpdateFromOneC_KeepsThePlaceByHandAndRemembersTheNewOneCParent()
    {
        var category = OneC(5513, "n1.n5513", "hardware");
        category.MoveUnder(null, Now);

        var changed = category.UpdateFromOneC("Ручки нові", CategoryPath.Create("n3.n5513"), ProductCategoryId.Create(3), Now);

        Assert.True(changed.Value);
        Assert.Equal("Ручки нові", category.Name);
        Assert.Null(category.ParentId);
        Assert.Equal("n5513", category.Path.Value);
        Assert.Equal(ProductCategoryId.Create(3), category.OneCParentId);
    }

    [Fact]
    public void UpdateFromOneC_DoesNotCountAMovedAncestorAsAChange()
    {
        var category = OneC(6000, "n9.n5513.n6000", "hardware");

        var changed = category.UpdateFromOneC(category.Name, CategoryPath.Create("n1.n5513.n6000"),
            ProductCategoryId.Create(5513), Now);

        Assert.False(changed.Value);
        Assert.Equal("n9.n5513.n6000", category.Path.Value);
    }

    [Fact]
    public void Order_ByHandSurvivesThePresentationFromOneC()
    {
        var category = OneC(5513, "n1.n5513", "hardware");
        category.SetOrder(3, Now);

        category.UpdatePresentationMetadata("Ручки", "Ручки", 40, 1);

        Assert.Equal(3, category.SortOrder);
        Assert.True(category.IsOrderedByHand);
        Assert.False(category.SetOrder(3, Now).Value);
    }

    [Fact]
    public void Rebuild_PutsANewOneCSubcategoryUnderItsMovedParent()
    {
        var root = OneC(1, "n1", "hardware");
        var moved = OneC(5513, "n1.n5513", "hardware");
        var target = OneC(2, "n2", "hardware");
        moved.MoveUnder(target, Now);
        var fresh = OneC(6000, "n1.n5513.n6000", "hardware");

        Assert.Equal(1, CategoryTreePlacement.Rebuild([root, moved, target, fresh], Now));
        Assert.Equal("n2.n5513.n6000", fresh.Path.Value);
        Assert.Equal(2, fresh.Level);
    }

    private static ProductCategory OneC(int id, string path, string root)
    {
        var segments = path.Split('.');
        ProductCategoryId? parentId = segments.Length > 1
            ? ProductCategoryId.Create(int.Parse(segments[^2][1..]))
            : null;
        return ProductCategory.Create(ProductCategoryId.Create(id), root, $"Категорія {id}", $"c-{id}",
            CategoryPath.Create(path), parentId).Value;
    }
}
