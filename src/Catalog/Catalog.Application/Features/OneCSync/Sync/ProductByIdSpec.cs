namespace Catalog.Application.Features.OneCSync.Sync;

public sealed class ProductByIdSpec : Specification<Product>
{
    public ProductByIdSpec(ProductId id)
    {
        Query.Where(p => p.Id == id);
    }
}
