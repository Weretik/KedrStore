namespace Application.Catalog.Specifications;

public class ProductsForCountSpecification : Specification<Product>
{
    public ProductsForCountSpecification(ProductsCriteria criteria)
    {
        Query.AsNoTracking().ApplyCommonFilters(criteria);
    }
}
