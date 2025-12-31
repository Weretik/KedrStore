using Catalog.Domain.Entities;
using Catalog.Domain.Enumerations;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Import.Queries.ImportCatalogFromXml;

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
