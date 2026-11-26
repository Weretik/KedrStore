using Catalog.Domain.Categories.Tree;

namespace Catalog.Domain.Categories;

/// <summary>
/// Where a category sits and in what order (spec catalog/012). An admin's place and order win over 1C: the sync keeps
/// them and only remembers the 1C parent. A category made by hand has no 1C parent at all.
/// </summary>
public partial class ProductCategory
{
    /// <summary>The root key of a manual category at the top of the tree; under a parent it takes the parent's root.</summary>
    public const string ManualRootKey = "manual";

    /// <summary>Made by hand in the admin, not by 1C or the configured groups (MC-01).</summary>
    public bool IsManual { get; private set; }

    /// <summary>The parent 1C gives (after the configured groups); the place to go back to (MC-09).</summary>
    public ProductCategoryId? OneCParentId { get; private set; }

    /// <summary>An admin put this 1C category under another parent; the sync keeps <see cref="ParentId"/> (MC-09).</summary>
    public bool IsPlacedByHand { get; private set; }

    /// <summary>An admin set the order among its neighbours; the sync keeps <see cref="SortOrder"/> (MC-10).</summary>
    public bool IsOrderedByHand { get; private set; }

    /// <summary>
    /// A manual category under <paramref name="parent"/> or at the top (MC-01, MC-02). Its names are both the source and
    /// the site names; visibility and search closing come from the parent unless <paramref name="isShownOnSite"/> is set.
    /// </summary>
    public static DomainResult<ProductCategory> CreateManual(ProductCategoryId id, string nameUk, string nameRu,
        string slug, ProductCategory? parent, bool? isShownOnSite, DateTimeOffset now)
    {
        if (parent is { IsDeleted: true })
            return CategoryErrors.ParentDeleted();

        var checks = new DomainChecks();
        var checkedUk = checks.Take(CheckShortName(nameUk, nameof(ShortNameUk)));
        var checkedRu = checks.Take(CheckShortName(nameRu, nameof(ShortNameRu)));
        if (checks.Error is { } error)
            return error;

        var path = parent is null ? CategoryPath.Root(id.Value.ToString()) : parent.Path.Append(id.Value.ToString());
        var created = Create(id, parent?.ProductTypeIdOneC ?? ManualRootKey, checkedUk, slug, path, parent?.Id);
        if (created.IsFailure)
            return created.Error!;

        var category = created.Value;
        category.IsManual = true;
        category.OneCParentId = null;
        var names = category.SetSiteNames(checkedUk, checkedRu, checkedUk, checkedRu, now);
        if (names.IsFailure)
            return names.Error!;
        category.IsShownOnSite = isShownOnSite ?? parent?.IsShownOnSite ?? true;
        category.IsClosedFromSearch = parent?.IsClosedFromSearch ?? false;
        category.UpdatedAt = now;
        return category;
    }

    /// <summary>Deleting by hand is only for a manual category (MC-04).</summary>
    public DomainResult EnsureManual() => IsManual ? DomainResult.Success() : CategoryErrors.NotManual();

    /// <summary>
    /// Puts the category under <paramref name="parent"/> or at the top (MC-03, MC-09). A 1C category put back under its
    /// 1C parent is no longer «placed by hand». Gives the old path when the place changed (the subcategories then follow
    /// through <see cref="CategoryTreePlacement"/>), <c>null</c> when it stayed.
    /// </summary>
    public DomainResult<CategoryPath?> MoveUnder(ProductCategory? parent, DateTimeOffset now)
    {
        if (parent is not null &&
            (parent.Id == Id || parent.Path.Value.StartsWith(Path.Value + ".", StringComparison.Ordinal)))
            return CategoryErrors.MoveIntoItself();
        if (parent is { IsDeleted: true })
            return CategoryErrors.ParentDeleted();
        if (ParentId == parent?.Id)
            return (CategoryPath?)null;

        var oldPath = Path;
        if (!IsManual)
            IsPlacedByHand = parent?.Id != OneCParentId;
        Place(parent, now);
        return (CategoryPath?)oldPath;
    }

    /// <summary>
    /// Sets the place from the parent's current path, for the whole tree after a move or a sync (MC-03, MC-05). Returns
    /// whether anything changed.
    /// </summary>
    public bool FollowParent(ProductCategory? parent, DateTimeOffset now)
    {
        var path = parent is null ? CategoryPath.Root(Id.Value.ToString()) : parent.Path.Append(Id.Value.ToString());
        var root = IsManual ? parent?.ProductTypeIdOneC ?? ManualRootKey : ProductTypeIdOneC;
        if (path.Value == Path.Value && root == ProductTypeIdOneC && ParentId == parent?.Id)
            return false;
        Place(parent, now);
        return true;
    }

    /// <summary>Sets the order among the neighbours by hand (MC-10). Returns whether it changed.</summary>
    public DomainResult<bool> SetOrder(int sortOrder, DateTimeOffset now)
    {
        if (sortOrder < 0)
            return CategoryErrors.SortOrderNegative();
        if (IsOrderedByHand && SortOrder == sortOrder)
            return false;
        IsOrderedByHand = true;
        SortOrder = sortOrder;
        UpdatedAt = now;
        return true;
    }

    private void Place(ProductCategory? parent, DateTimeOffset now)
    {
        Path = parent is null ? CategoryPath.Root(Id.Value.ToString()) : parent.Path.Append(Id.Value.ToString());
        ParentId = parent?.Id;
        // A 1C category keeps its root: the sync finds the categories of a root by it.
        if (IsManual)
            ProductTypeIdOneC = parent?.ProductTypeIdOneC ?? ManualRootKey;
        Level = GetLevelFromPath(Path);
        UpdatedAt = now;
    }
}
