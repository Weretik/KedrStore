namespace Application.UseCases.Catalog.DTOs;

public class CategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public int? ParentCategoryId { get; init; }
    public List<CategoryDto>? Children { get; init; }
}
