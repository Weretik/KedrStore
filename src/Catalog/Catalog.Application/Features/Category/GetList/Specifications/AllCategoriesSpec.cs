namespace Catalog.Application.Features.Category.GetList.Specifications;

public sealed class AllCategoriesSpec : Specification<ProductCategory>
{
    public AllCategoriesSpec(int? productTypeId)
    {
        Query.AsNoTracking();


        Query.OrderBy(category => category.Path)
            .ThenBy(category => category.Name);
    }
}
