using Domain.Catalog.Entities;
using Domain.Catalog.ValueObjects;

namespace Application.Catalog.ImportCatalogFromXml;

public sealed class ProductsMissingInImportSpec : Specification<Product>
{
    public ProductsMissingInImportSpec(ProductId[] importProductIds)
    {
        Query.IgnoreQueryFilters()
            .Where(product => !importProductIds.Contains(product.Id));
    }
}
