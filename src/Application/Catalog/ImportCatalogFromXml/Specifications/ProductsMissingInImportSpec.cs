using Domain.Catalog.Entities;
using Domain.Catalog.Enumerations;
using Domain.Catalog.ValueObjects;

namespace Application.Catalog.ImportCatalogFromXml;

public sealed class ProductsMissingInImportSpec : Specification<Product>
{
    public ProductsMissingInImportSpec(ProductId[] importProductIds, int productTypeId)
    {
        Query.IgnoreQueryFilters()
            .Where(product =>
                product.ProductType == ProductType.FromValue(productTypeId)
                && !importProductIds.Contains(product.Id));
    }
}
