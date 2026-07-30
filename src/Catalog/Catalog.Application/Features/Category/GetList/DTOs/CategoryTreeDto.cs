namespace Catalog.Application.Features.Category.GetList.DTOs;

public sealed record CategoryTreeDto(
    int Id,
    string Slug,
    string Name,
    int? ParentId,
    string ShortNameUk,
    string ShortNameRu,
    int SortOrder,
    int Level,
    IReadOnlyList<CategoryTreeDto> Children);
