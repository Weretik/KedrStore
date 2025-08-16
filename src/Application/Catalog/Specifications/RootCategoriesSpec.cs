namespace Application.Catalog.Specifications;

public sealed class RootCategoriesSpec : Specification<Category>
{
    public RootCategoriesSpec()
    {
        Query.Where(c => c.ParentCategoryId == null)
            .AsNoTracking();
    }
}
