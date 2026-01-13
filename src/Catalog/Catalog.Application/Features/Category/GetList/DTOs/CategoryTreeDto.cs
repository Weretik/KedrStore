namespace Catalog.Application.Features.Category.GetList;

public sealed record CategoryTreeDto(int Id, string Name, IReadOnlyList<CategoryTreeDto> Children);
