namespace Catalog.Application.Features.OneCSync.Sync.Products;

public sealed class ProductByIdSpec : Specification<Product>
{
    public ProductByIdSpec(ProductId id)
    {
        Query.Where(p => p.Id == id);
    }
}
