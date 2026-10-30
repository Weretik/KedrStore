namespace Catalog.Contracts.Category;

public sealed record CategoryResponse(
    int Id,
    string Slug,
    string Name,
    int? ParentId,
    int SortOrder,
    int Level,
    IReadOnlyList<CategoryResponse> Children,
    int ProductCount);

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
    CategoryContentResponse? Content);

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
    DateTimeOffset? DeletedAt);

public sealed record CategoryFaqItemDto(string Question, string Answer);

/// <summary>Public category content in the requested language.</summary>
public sealed record CategoryContentResponse(
    string? SeoTitle,
    string? MetaDescription,
    string? H1,
    string? BodyHtml,
    IReadOnlyList<CategoryFaqItemDto> Faq,
    DateTimeOffset UpdatedAt);

/// <summary>Admin view of one language, including who changed it.</summary>
public sealed record AdminCategoryContentItem(
    string Language,
    string? SeoTitle,
    string? MetaDescription,
    string? H1,
    string? BodyHtml,
    IReadOnlyList<CategoryFaqItemDto> Faq,
    DateTimeOffset UpdatedAt,
    string UpdatedBy);

public sealed record AdminCategoryContentResponse(int CategoryId, IReadOnlyList<AdminCategoryContentItem> Items);

/// <summary>Full replacement of one language; empty fields clear them.</summary>
public sealed record SaveCategoryContentRequest(
    string? SeoTitle,
    string? MetaDescription,
    string? H1,
    string? BodyHtml,
    IReadOnlyList<CategoryFaqItemDto>? Faq);
