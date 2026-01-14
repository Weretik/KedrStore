using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Specifications;

public sealed class ProductPriceByProductAndTypeSpec : Specification<ProductPrice>
{
    public ProductPriceByProductAndTypeSpec(ProductId productId, PriceTypeId priceTypeId)
    {
        Query.Where(p => p.ProductId == productId && p.PriceTypeId == priceTypeId);
    }
}
