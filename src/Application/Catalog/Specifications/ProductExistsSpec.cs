namespace Application.Catalog.Specifications;

public sealed class ProductExistsSpec : Specification<Product>
{
    public ProductExistsSpec(ProductId id)
    {
        Query.Where(p => p.Id == id);
    }
}
