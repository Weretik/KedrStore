using Domain.Catalog.Entities;
using Domain.Catalog.Enumerations;

namespace Application.Catalog.GetCategories;

public sealed class AllCategoriesSpec : Specification<ProductCategory>
{
    public AllCategoriesSpec(int? productTypeId)
    {
        Query.AsNoTracking();

        if (productTypeId.HasValue)
            Query.Where(x =>
                x.ProductType == ProductType.FromValue(productTypeId.Value));

        Query.OrderBy(category => category.Path)
            .ThenBy(category => category.Name);
    }
}
