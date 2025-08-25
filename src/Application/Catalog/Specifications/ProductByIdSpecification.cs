namespace Application.Catalog.Specifications;

public sealed class ProductByIdSpecification : Specification<Product>, ISingleResultSpecification<Product>
{
    public ProductByIdSpecification(ProductId id)
    {
        Query.Where(p => p.Id == id)
            .AsNoTracking();
    }
}
