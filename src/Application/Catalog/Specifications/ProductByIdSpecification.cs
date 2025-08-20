namespace Application.Catalog.Specifications;

public sealed class ProductByIdSpecification : Specification<Product>, ISingleResultSpecification
{
    public ProductByIdSpecification(ProductId id)
    {
        Query.Where(p => p.Id == id)
            .AsNoTracking();
    }
}
