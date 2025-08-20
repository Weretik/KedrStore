namespace Application.Catalog.Specifications;

public sealed class RootCategoriesSpecification : Specification<Category>
{
    public RootCategoriesSpecification()
    {
        Query.Where(c => c.ParentCategoryId == null)
            .AsNoTracking();
    }
}
