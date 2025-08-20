namespace Application.Catalog.Specifications;

public sealed class CategoryByIdSpecification : Specification<Category>, ISingleResultSpecification
{
    public CategoryByIdSpecification(CategoryId id)
    {
        Query.Where(c => c.Id == id)
            .AsNoTracking();
    }
}
