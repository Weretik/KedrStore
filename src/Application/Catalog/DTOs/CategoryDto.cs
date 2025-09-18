namespace Application.Catalog.DTOs;

public class CategoryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public int? ParentCategoryId { get; init; }
    public IReadOnlyList<CategoryDto> Children { get; init; } = [];
}
