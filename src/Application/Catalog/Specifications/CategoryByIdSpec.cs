namespace Application.Catalog.Specifications;

public sealed class CategoryByIdSpec : Specification<Category>, ISingleResultSpecification
{
    public CategoryByIdSpec(CategoryId id)
    {
        Query.Where(c => c.Id == id)
            .AsNoTracking();
    }
}
