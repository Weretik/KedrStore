namespace Application.Catalog.Specifications;

public sealed class CategoryExistsSpecification : Specification<ProductCategory>
{
    public CategoryExistsSpecification(CategoryId id)
    {
        Query.Where(c => c.Id == id);
    }
}
