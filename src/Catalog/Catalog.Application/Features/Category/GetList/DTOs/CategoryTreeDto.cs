namespace Catalog.Application.Features.Category.GetList.DTOs;

public sealed record CategoryTreeDto(int Id, string Name, IReadOnlyList<CategoryTreeDto> Children);
