namespace Catalog.Application.Features.OneCSync.Sync.Prices;

public sealed class ProductPriceByProductAndTypeSpec : Specification<ProductPrice>
{
    public ProductPriceByProductAndTypeSpec(ProductId productId, PriceTypeId priceTypeId)
    {
        Query.Where(p => p.ProductId == productId && p.PriceTypeId == priceTypeId);
    }
}
