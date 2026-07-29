using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Specifications;

public sealed class ProductsByIdsFromOtherRootsSpec : Specification<Product>
{
    public ProductsByIdsFromOtherRootsSpec(IEnumerable<ProductId> ids, string excludedProductTypeIdOneC)
    {
        Query.Where(product => ids.Contains(product.Id) && product.ProductTypeIdOneC != excludedProductTypeIdOneC);
    }
}
