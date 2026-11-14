namespace Catalog.Application.Features.Categories.Shared.Tree;

public sealed class AllCategoriesSpec : Specification<ProductCategory>
{
    public AllCategoriesSpec(int? productTypeId)
    {
        Query.AsNoTracking();


        Query.OrderBy(category => category.Path)
            .ThenBy(category => category.Name);
    }
}
