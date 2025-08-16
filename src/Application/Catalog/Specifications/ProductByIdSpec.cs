namespace Application.Catalog.Specifications;

public sealed class ProductByIdSpec : Specification<Product>, ISingleResultSpecification
{
    public ProductByIdSpec(ProductId id)
    {
        Query.Where(p => p.Id == id)
            .AsNoTracking();
    }
}
