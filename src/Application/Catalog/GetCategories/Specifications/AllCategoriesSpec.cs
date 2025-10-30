using Domain.Catalog.Entities;

namespace Application.Catalog.GetCategories;

public sealed class AllCategoriesSpec : Specification<ProductCategory>
{
    public AllCategoriesSpec()
    {
        Query.AsNoTracking().OrderBy(c => c.Name);
    }
}
