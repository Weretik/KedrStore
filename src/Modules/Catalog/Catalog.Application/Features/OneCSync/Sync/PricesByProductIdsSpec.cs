namespace Catalog.Application.Features.OneCSync.Sync;

public sealed class PricesByProductIdsSpec : Specification<ProductPrice>
{
    public PricesByProductIdsSpec(ProductId[] productIds,  string productTypeIdOneC)
    {
        Query.Where(p => productIds.Contains(p.ProductId) && p.ProductTypeIdOneC == productTypeIdOneC);
    }
}
