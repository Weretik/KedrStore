namespace Catalog.Application.Features.OneCSync.Sync.Products;

public sealed class ProductsByIdsFromOtherRootsSpec : Specification<Product>
{
    public ProductsByIdsFromOtherRootsSpec(IEnumerable<ProductId> ids, string excludedProductTypeIdOneC)
    {
        Query.Where(product => ids.Contains(product.Id) && product.ProductTypeIdOneC != excludedProductTypeIdOneC);
    }
}
