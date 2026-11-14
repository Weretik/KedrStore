namespace Catalog.Application.Features.Categories.Queries.GetCategories;

public sealed record CategoryTreeDto(
    int Id,
    string Slug,
    string Name,
    int? ParentId,
    string ShortNameUk,
    string ShortNameRu,
    int SortOrder,
    int Level,
    IReadOnlyList<CategoryTreeDto> Children,
    int ProductCount = 0);
