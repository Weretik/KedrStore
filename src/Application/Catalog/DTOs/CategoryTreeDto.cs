namespace Application.Catalog.DTOs;

public sealed record CategoryTreeDto(int Id, string Name, IReadOnlyList<CategoryTreeDto> Children);
