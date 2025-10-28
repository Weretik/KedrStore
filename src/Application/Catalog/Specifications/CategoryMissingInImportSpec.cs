namespace Application.Catalog.Specifications;

public sealed class CategoryMissingInImportSpec: Specification<ProductCategory>
{
    public CategoryMissingInImportSpec(ISet<int> importIds)
    {
        Query.IgnoreQueryFilters()
            .Where(productCategory => !importIds.Contains(productCategory.Id.Value))
            .OrderByDescending(productCategory => productCategory.Path.NLevel);
    }
}
