using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Specifications;

public sealed class PricesByProductIdsSpec : Specification<ProductPrice>
{
    public PricesByProductIdsSpec(ProductId[] productIds,  string productTypeIdOneC)
    {
        Query.Where(p => productIds.Contains(p.ProductId) && p.ProductTypeIdOneC == productTypeIdOneC);
    }
}
