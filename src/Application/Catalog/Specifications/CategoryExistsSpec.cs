namespace Application.Catalog.Specifications;

public sealed class CategoryExistsSpec : Specification<Category>
{
    public CategoryExistsSpec(CategoryId id)
    {
        Query.Where(c => c.Id == id);
    }
}
