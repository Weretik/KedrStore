namespace Application.Catalog.Specifications;

public class ProductsForCountSpecification : Specification<Product>
{
    public ProductsForCountSpecification(ProductsFilter filter)
    {
        Query.AsNoTracking().ApplyCommonFilters(filter);
    }
}
