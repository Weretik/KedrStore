namespace Application.Catalog.Specifications;

public sealed class CategoryByIdSpecification : Specification<Category>, ISingleResultSpecification<Category>
{
    public CategoryByIdSpecification(CategoryId id)
    {
        Query.Where(c => c.Id == id)
            .AsNoTracking();
    }
}
