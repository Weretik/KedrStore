namespace Application.Catalog.Specifications;

public sealed class ProductsMissingInImportSpec : Specification<Product>
{
    public ProductsMissingInImportSpec(ISet<int> importIds)
    {
        Query.IgnoreQueryFilters()
            .Where(product => !importIds.Contains(product.Id.Value));
    }
}
