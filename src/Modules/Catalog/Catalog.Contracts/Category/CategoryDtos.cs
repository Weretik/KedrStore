namespace Catalog.Contracts.Category;

public sealed record CategoryResponse(
    int Id,
    string Slug,
    string Name,
    int? ParentId,
    int SortOrder,
    int Level,
    IReadOnlyList<CategoryResponse> Children,
    int ProductCount)
{
    /// <summary>«Закрити від пошуку» (spec catalog/011 R-01): the site gives the page <c>noindex</c> and leaves it out of the sitemap.</summary>
    public bool IsClosedFromSearch { get; init; }
    /// <summary>When an admin or 1C last changed the category itself; the sitemap's <c>lastmod</c> (R-04).</summary>
    public DateTimeOffset? UpdatedAt { get; init; }
}

public sealed record CategoryBreadcrumbResponse(int Id, string Slug, string Name);

public sealed record CategoryDetailsResponse(
    int Id,
    string Slug,
    string Name,
    int? ParentId,
    int SortOrder,
    int Level,
    IReadOnlyList<CategoryResponse> Children,
    IReadOnlyList<CategoryBreadcrumbResponse> Breadcrumbs,
    int ProductCount,
    decimal? MinPrice,
    decimal? MaxPrice,
    CategoryContentResponse? Content)
{
    /// <summary>«Закрити від пошуку» (spec catalog/011 R-01): the page and its products get <c>noindex</c>.</summary>
    public bool IsClosedFromSearch { get; init; }
    /// <summary>CDN address of the link-preview picture; <c>null</c> keeps the site's own (R-03).</summary>
    public string? ShareImageUrl { get; init; }
    /// <summary>The later of the category's and this language's content change (R-04).</summary>
    public DateTimeOffset? UpdatedAt { get; init; }
}

public sealed record AdminCategoryResponse(
    int Id,
    string Name,
    string ShortNameUk,
    string ShortNameRu,
    string Slug,
    string ProductTypeIdOneC,
    int? ParentId,
    int SortOrder,
    int Level,
    bool IsDeleted,
    DateTimeOffset? DeletedAt,
    bool IsShownOnSite,
    int? ProductCount = null,
    IReadOnlyList<string>? FilledContentLanguages = null)
{
    /// <summary>Names an admin wrote for the site; they win over 1C (spec catalog/011 R-02). Empty when not written.</summary>
    public string? SiteNameUk { get; init; }
    public string? SiteNameRu { get; init; }
    /// <summary>«Закрити від пошуку»: the site shows the category and its products with <c>noindex</c> (R-01).</summary>
    public bool IsClosedFromSearch { get; init; }
    public long? ShareImageMediaFileId { get; init; }
    /// <summary>CDN address of the link-preview picture; only the by-id read and the save fill it (R-03).</summary>
    public string? ShareImageUrl { get; init; }
    /// <summary>Made by hand in the admin (spec catalog/012 MC-01): only such a category can be deleted.</summary>
    public bool IsManual { get; init; }
}

public sealed record CategoryFaqItemDto(string Question, string Answer);

/// <summary>Public category content in the requested language.</summary>
public sealed record CategoryContentResponse(
    string? SeoTitle,
    string? MetaDescription,
    string? H1,
    string? BodyHtml,
    IReadOnlyList<CategoryFaqItemDto> Faq,
    DateTimeOffset UpdatedAt);

/// <summary>Admin view of one language, including who changed it; <c>Version</c> goes back with the next save.</summary>
public sealed record AdminCategoryContentItem(
    string Language,
    string? SeoTitle,
    string? MetaDescription,
    string? H1,
    string? BodyHtml,
    IReadOnlyList<CategoryFaqItemDto> Faq,
    DateTimeOffset UpdatedAt,
    string UpdatedBy,
    uint Version);

public sealed record AdminCategoryContentResponse(int CategoryId, IReadOnlyList<AdminCategoryContentItem> Items);

/// <summary>
/// Full replacement of one language; empty fields clear them. <c>Version</c> is the one the form read (0 for a
/// language never saved): when the content changed since, the save answers 409; without it the save is not checked.
/// </summary>
public sealed record SaveCategoryContentRequest(
    string? SeoTitle,
    string? MetaDescription,
    string? H1,
    string? BodyHtml,
    IReadOnlyList<CategoryFaqItemDto>? Faq,
    uint? Version = null);

/// <summary>A category shown or hidden on the site; <c>ChangedCount</c> counts it with its affected relatives.</summary>
public sealed record CategorySiteVisibilityResponse(int Id, bool IsShownOnSite, int ChangedCount);

/// <summary>
/// The admin's site names of a category (spec catalog/011 R-02); a blank one goes back to the name from 1C and the
/// configuration.
/// </summary>
public sealed record SetCategorySiteNamesRequest(string? NameUk, string? NameRu);

/// <summary>
/// «Закрити від пошуку» for the category and every subcategory, and the link-preview picture of the category page
/// (spec catalog/011 R-01, R-03). <c>ShareImageMediaFileId</c> <c>null</c> removes the picture.
/// </summary>
public sealed record SetCategorySearchSettingsRequest
{
    public bool IsClosedFromSearch { get; init; }
    public long? ShareImageMediaFileId { get; init; }
}

/// <summary>Shows or hides a category, with its subcategories, on the site (spec catalog/007 R-18).</summary>
public sealed record SetCategorySiteVisibilityRequest
{
    public bool IsShownOnSite { get; init; }
}
