using Domain.Catalog.Entities;

namespace Application.Catalog.GetCategories;

public sealed class AllCategoriesSpec : Specification<ProductCategory>
{
    public AllCategoriesSpec()
    {
        Query.AsNoTracking()
            .OrderBy(category => category.Path)
            .ThenBy(category => category.Name);
    }
}
