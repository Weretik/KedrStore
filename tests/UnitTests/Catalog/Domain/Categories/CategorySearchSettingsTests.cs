using Catalog.Domain.Categories.Tree;
using BuildingBlocks.Domain.Identifiers;
using Catalog.Domain.Categories;

namespace UnitTests.Catalog.Domain.Categories;

/// <summary>Spec catalog/011: «Закрити від пошуку», the admin's site names, and the link-preview picture.</summary>
public sealed class CategorySearchSettingsTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Close_ClosesTheBranchIncludingDeletedSubcategoriesButNotParentsOrLookalikes()
    {
        var root = Category(1, "n1");
        var selected = Category(2, "n1.n2", 1);
        var deletedChild = Category(3, "n1.n2.n3", 2);
        deletedChild.MarkDeleted(Now);
        var lookalike = Category(22, "n1.n22", 1);

        var changed = CategorySearchClosing.Apply(selected, [root, selected, deletedChild, lookalike], true, Now);

        Assert.Equal(2, changed);
        Assert.True(selected.IsClosedFromSearch);
        Assert.True(deletedChild.IsClosedFromSearch);
        Assert.False(root.IsClosedFromSearch);
        Assert.False(lookalike.IsClosedFromSearch);
        Assert.Equal(Now, selected.UpdatedAt);
    }

    [Fact]
    public void Open_OpensTheBranch_AndRepeatingChangesNothing()
    {
        var selected = Category(1, "n1");
        var child = Category(2, "n1.n2", 1);
        CategorySearchClosing.Apply(selected, [selected, child], true, Now);

        Assert.Equal(2, CategorySearchClosing.Apply(selected, [selected, child], false, Now));
        Assert.False(child.IsClosedFromSearch);
        Assert.Equal(0, CategorySearchClosing.Apply(selected, [selected, child], false, Now));
    }

    [Fact]
    public void SiteNames_WinOverTheOneCNameAndSurviveTheNextSync()
    {
        var category = Category(1, "n1");

        var set = category.SetSiteNames(" Ручки ", "Ручки дверные", "Категорія 1", "Категорія 1", Now);

        Assert.True(set.Value);
        Assert.Equal("Ручки", category.SiteNameUk);
        Assert.Equal("Ручки", category.ShortNameUk);
        Assert.Equal("Ручки дверные", category.ShortNameRu);

        var synced = category.UpdatePresentationMetadata("Ручки з 1С", "Ручки из 1С", 3, 0);

        Assert.True(synced.Value);
        Assert.Equal("Ручки", category.ShortNameUk);
        Assert.Equal("Ручки дверные", category.ShortNameRu);
        Assert.Equal(3, category.SortOrder);
    }

    [Fact]
    public void BlankSiteName_GoesBackToTheSourceName()
    {
        var category = Category(1, "n1");
        category.SetSiteNames("Ручки", null, "Категорія 1", "Категорія 1", Now);

        var cleared = category.SetSiteNames("  ", null, "Ручки (налаштування)", "Ручки (настройка)", Now);

        Assert.True(cleared.Value);
        Assert.Null(category.SiteNameUk);
        Assert.Equal("Ручки (налаштування)", category.ShortNameUk);
        Assert.Equal("Ручки (настройка)", category.ShortNameRu);
    }

    [Fact]
    public void SiteName_LongerThanTheLimit_IsRefusedAndNothingChanges()
    {
        var category = Category(1, "n1");

        var result = category.SetSiteNames(new string('я', ProductCategory.ShortNameMaxLength + 1), null,
            "Категорія 1", "Категорія 1", Now);

        Assert.True(result.IsFailure);
        Assert.Equal("SiteNameUk", result.Error!.Field);
        Assert.Null(category.SiteNameUk);
        Assert.Equal("Категорія 1", category.ShortNameUk);
    }

    [Fact]
    public void ShareImage_IsSetRemovedAndRefusedWhenInvalid()
    {
        var category = Category(1, "n1");

        Assert.True(category.SetShareImage(MediaFileId.FromStorage(7), Now).Value);
        Assert.False(category.SetShareImage(MediaFileId.FromStorage(7), Now).Value);
        Assert.Equal(7, category.ShareImageMediaFileId?.Value);
        Assert.True(category.SetShareImage(null, Now).Value);
        Assert.Null(category.ShareImageMediaFileId);
        Assert.True(category.SetShareImage(MediaFileId.FromStorage(0), Now).IsFailure);
    }

    private static ProductCategory Category(int id, string path, int? parentId = null)
        => ProductCategory.Create(ProductCategoryId.Create(id), $"{id:D9}", $"Категорія {id}", $"category-{id}",
            CategoryPath.Create(path), parentId is { } parent ? ProductCategoryId.Create(parent) : null).Value;
}
