namespace Catalog.Domain.Categories;

public class ProductCategory : BaseEntity<ProductCategoryId>, IAggregateRoot
{
    public const int ShortNameMaxLength = 100;

    #region Properties
    public string ProductTypeIdOneC { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public ProductCategoryId? ParentId { get; private set; }
    public CategoryPath  Path { get; private set; }
    public string ShortNameUk { get; private set; } = null!;
    public string ShortNameRu { get; private set; } = null!;
    public int SortOrder { get; private set; }
    public int Level { get; private set; }
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// Whether the category and its products are shown on the site, in «Вітрина сайту», and in «Моделі». An admin owns
    /// it; the 1C sync never changes it and gives a new category its parent's value.
    /// </summary>
    public bool IsShownOnSite { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    #endregion

    public const int NameMaxLength = 100;

    #region Constructors
    private ProductCategory() { }
    #endregion

    #region Factories
    public static DomainResult<ProductCategory> Create(ProductCategoryId id, string productTypeIdOneC, string name,
        string slug, CategoryPath path, ProductCategoryId? parentId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productTypeIdOneC);
        var checks = new DomainChecks();
        var checkedName = checks.Take(CheckName(name));
        var checkedSlug = checks.Take(CheckSlug(slug));
        checks.Check(CheckPath(path));
        if (checks.Error is { } error)
            return error;

        var category = new ProductCategory
        {
            Id = id,
            ProductTypeIdOneC = productTypeIdOneC,
            Name = checkedName,
            Slug = checkedSlug,
            Path = path,
            ParentId = parentId,
        };
        var metadata = category.UpdatePresentationMetadata(checkedName, checkedName, int.MaxValue,
            GetLevelFromPath(path));
        return metadata.IsFailure ? metadata.Error! : category;
    }

    /// <summary>
    /// Applies 1C-owned fields. The slug is not 1C-owned and never changes here.
    /// Returns whether anything changed.
    /// </summary>
    public DomainResult<bool> UpdateFromOneC(string name, CategoryPath path, ProductCategoryId? parentId,
        DateTimeOffset now)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (string.Equals(Name, trimmedName, StringComparison.Ordinal) &&
            Path.Value == path.Value &&
            ParentId == parentId)
        {
            return false;
        }

        var checks = new DomainChecks();
        var checkedName = checks.Take(CheckName(name!));
        checks.Check(CheckPath(path));
        if (checks.Error is { } error)
            return error;

        Name = checkedName;
        Path = path;
        ParentId = parentId;
        Level = GetLevelFromPath(path);
        UpdatedAt = now;
        return true;
    }
    #endregion

    #region Lifecycle
    public DomainResult ChangeSlug(string slug, DateTimeOffset now)
    {
        var checkedSlug = CheckSlug(slug);
        if (checkedSlug.IsFailure)
            return checkedSlug;
        Slug = checkedSlug.Value;
        UpdatedAt = now;
        return DomainResult.Success();
    }

    public bool MarkDeleted(DateTimeOffset now)
    {
        if (IsDeleted) return false;
        IsDeleted = true;
        DeletedAt = now;
        UpdatedAt = now;
        return true;
    }

    /// <summary>Shows or hides the category on the site; returns whether it changed.</summary>
    public bool SetShownOnSite(bool isShown, DateTimeOffset now)
    {
        if (IsShownOnSite == isShown) return false;
        IsShownOnSite = isShown;
        UpdatedAt = now;
        return true;
    }

    public bool Restore(DateTimeOffset now)
    {
        if (!IsDeleted) return false;
        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = now;
        return true;
    }
    #endregion

    #region Validation
    private static DomainResult<string> CheckName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return CategoryErrors.NameRequired();
        var trimmed = name.Trim();
        return trimmed.Length > NameMaxLength ? CategoryErrors.NameTooLong(trimmed.Length) : trimmed;
    }

    private static DomainResult<string> CheckSlug(string slug)
        => string.IsNullOrWhiteSpace(slug) ? CategoryErrors.SlugRequired() : slug.Trim();

    private static DomainResult CheckPath(CategoryPath path)
        => path.Value.Length == 0 ? CategoryErrors.PathRequired() : DomainResult.Success();

    private static DomainResult<string> CheckShortName(string shortName, string field)
    {
        if (string.IsNullOrWhiteSpace(shortName))
            return CategoryErrors.ShortNameRequired(field);

        var trimmed = shortName.Trim();
        return trimmed.Length > ShortNameMaxLength ? CategoryErrors.ShortNameTooLong(field, trimmed.Length) : trimmed;
    }

    private DomainResult CheckLevel(int level)
    {
        if (level < 0)
            return CategoryErrors.LevelNegative();

        var pathLevel = GetLevelFromPath(Path);
        return level != pathLevel ? CategoryErrors.LevelDoesNotMatchPath(pathLevel) : DomainResult.Success();
    }
    #endregion

    #region ProductCategory API
    /// <summary>Sets presentation metadata and returns whether anything changed.</summary>
    public DomainResult<bool> UpdatePresentationMetadata(string shortNameUk, string shortNameRu, int sortOrder,
        int level)
    {
        var checks = new DomainChecks();
        var validatedShortNameUk = checks.Take(CheckShortName(shortNameUk, nameof(ShortNameUk)));
        var validatedShortNameRu = checks.Take(CheckShortName(shortNameRu, nameof(ShortNameRu)));
        if (sortOrder < 0)
            checks.Check(CategoryErrors.SortOrderNegative());
        checks.Check(CheckLevel(level));
        if (checks.Error is { } error)
            return error;

        var changed = !string.Equals(ShortNameUk, validatedShortNameUk, StringComparison.Ordinal) ||
                      !string.Equals(ShortNameRu, validatedShortNameRu, StringComparison.Ordinal) ||
                      SortOrder != sortOrder ||
                      Level != level;

        ShortNameUk = validatedShortNameUk;
        ShortNameRu = validatedShortNameRu;
        SortOrder = sortOrder;
        Level = level;
        return changed;
    }
    #endregion

    #region Path rules
    public bool IsRoot => SegmentCount(Path) <= 1;

    private static int SegmentCount(CategoryPath path)
        => 1 + path.Value.Count(ch => ch == '.');

    private static int GetLevelFromPath(CategoryPath path)
        => SegmentCount(path) - 1;
    #endregion
}
