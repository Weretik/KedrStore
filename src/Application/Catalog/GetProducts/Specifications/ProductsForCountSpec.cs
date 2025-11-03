using Domain.Catalog.Entities;

namespace Application.Catalog.GetProducts;

public class ProductsForCountSpec : Specification<Product>
{
    public ProductsForCountSpec(ProductFilter filter, PricingOptions pricingOptions)
    {
        Query.AsNoTracking().ApplyFilters(filter, pricingOptions);
    }
}
