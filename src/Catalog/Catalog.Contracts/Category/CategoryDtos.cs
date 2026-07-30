namespace Catalog.Contracts.Category;

public sealed record CategoryResponse(
    int Id,
    string Slug,
    string Name,
    int? ParentId,
    int SortOrder,
    int Level,
    IReadOnlyList<CategoryResponse> Children);

public sealed record CategoryBreadcrumbResponse(int Id, string Slug, string Name);

public sealed record CategoryDetailsResponse(
    int Id,
    string Slug,
    string Name,
    int? ParentId,
    int SortOrder,
    int Level,
    IReadOnlyList<CategoryResponse> Children,
    IReadOnlyList<CategoryBreadcrumbResponse> Breadcrumbs);

public sealed record AdminCategoryResponse(
    int Id,
    string Name,
    string ShortNameUk,
    string ShortNameRu,
    string Slug,
    string ProductTypeIdOneC,
    int? ParentId,
    int SortOrder,
    int Level);
