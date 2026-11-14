namespace Catalog.Application.Features.Categories.Queries.GetCategories;

public sealed class AllCategoriesSpec : Specification<ProductCategory>
{
    public AllCategoriesSpec(int? productTypeId)
    {
        Query.AsNoTracking();


        Query.OrderBy(category => category.Path)
            .ThenBy(category => category.Name);
    }
}
