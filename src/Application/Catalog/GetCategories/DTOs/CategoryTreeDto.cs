namespace Application.Catalog.GetCategories;

public sealed record CategoryTreeDto(int Id, string Name, IReadOnlyList<CategoryTreeDto> Children);
