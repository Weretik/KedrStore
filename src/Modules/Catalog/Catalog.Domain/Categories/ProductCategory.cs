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

    /// <summary>
    /// Names an admin wrote for the site (spec catalog/010 R-02). They win over the 1C name and the configured short
    /// name: <see cref="ShortNameUk"/> and <see cref="ShortNameRu"/> always hold the name the site shows.
    /// </summary>
    public string? SiteNameUk { get; private set; }
    public string? SiteNameRu { get; private set; }

    /// <summary>
    /// Whether search engines must not index the category and its products (spec catalog/010 R-01): the site still shows
    /// them, with <c>noindex</c> and outside the sitemap. An admin owns it; a new 1C category takes its parent's value.
    /// </summary>
    public bool IsClosedFromSearch { get; private set; }

    /// <summary>The media-library picture for link previews of the category page; <c>null</c> keeps the site's own.</summary>
    public MediaFileId? ShareImageMediaFileId { get; private set; }
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

    /// <summary>Closes or opens the category for search engines; returns whether it changed.</summary>
    public bool SetClosedFromSearch(bool isClosed, DateTimeOffset now)
    {
        if (IsClosedFromSearch == isClosed) return false;
        IsClosedFromSearch = isClosed;
        UpdatedAt = now;
        return true;
    }

    /// <summary>Sets or removes the link-preview picture; returns whether it changed.</summary>
    public DomainResult<bool> SetShareImage(MediaFileId? mediaFileId, DateTimeOffset now)
    {
        if (mediaFileId is { Value: <= 0 })
            return CategoryErrors.ShareImageInvalid();
        if (ShareImageMediaFileId == mediaFileId) return false;
        ShareImageMediaFileId = mediaFileId;
        UpdatedAt = now;
        return true;
    }

    /// <summary>
    /// Sets the admin's site names; a blank one goes back to <paramref name="sourceUk"/> or <paramref name="sourceRu"/>,
    /// the name 1C and the configuration give. Returns whether the shown names changed.
    /// </summary>
    public DomainResult<bool> SetSiteNames(string? nameUk, string? nameRu, string sourceUk, string sourceRu,
        DateTimeOffset now)
    {
        var checks = new DomainChecks();
        var checkedUk = checks.Take(CheckSiteName(nameUk, nameof(SiteNameUk)));
        var checkedRu = checks.Take(CheckSiteName(nameRu, nameof(SiteNameRu)));
        if (checks.Error is { } error)
            return error;

        var namesChanged = !string.Equals(SiteNameUk, checkedUk, StringComparison.Ordinal) ||
                           !string.Equals(SiteNameRu, checkedRu, StringComparison.Ordinal);
        SiteNameUk = checkedUk;
        SiteNameRu = checkedRu;
        var shown = UpdatePresentationMetadata(sourceUk, sourceRu, SortOrder, Level);
        if (shown.IsFailure)
            return shown.Error!;
        if (namesChanged || shown.Value)
            UpdatedAt = now;
        return namesChanged || shown.Value;
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

    private static DomainResult<string?> CheckSiteName(string? name, string field)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (string?)null;
        var trimmed = name.Trim();
        return trimmed.Length > ShortNameMaxLength
            ? CategoryErrors.ShortNameTooLong(field, trimmed.Length)
            : trimmed;
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
    /// <summary>
    /// Sets presentation metadata from 1C and the configuration and returns whether anything changed. An admin's site
    /// name (<see cref="SiteNameUk"/>, <see cref="SiteNameRu"/>) stays the shown name.
    /// </summary>
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

        validatedShortNameUk = SiteNameUk ?? validatedShortNameUk;
        validatedShortNameRu = SiteNameRu ?? validatedShortNameRu;
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
