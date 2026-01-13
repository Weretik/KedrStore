using Catalog.Domain.Entities;

namespace Catalog.Application.Features.Category.GetList;

public sealed class AllCategoriesSpec : Specification<ProductCategory>
{
    public AllCategoriesSpec(int? productTypeId)
    {
        Query.AsNoTracking();


        Query.OrderBy(category => category.Path)
            .ThenBy(category => category.Name);
    }
}
