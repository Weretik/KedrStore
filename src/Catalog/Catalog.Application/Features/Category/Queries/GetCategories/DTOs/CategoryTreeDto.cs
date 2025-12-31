namespace Catalog.Application.Features.Category.Queries.GetCategories;

public sealed record CategoryTreeDto(int Id, string Name, IReadOnlyList<CategoryTreeDto> Children);
