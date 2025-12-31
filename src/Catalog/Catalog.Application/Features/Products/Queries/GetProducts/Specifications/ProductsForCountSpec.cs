using Catalog.Domain.Entities;

namespace Catalog.Application.Features.Products.Queries.GetProducts;

public class ProductsForCountSpec : Specification<Product>
{
    public ProductsForCountSpec(ProductFilter filter, PricingOptions pricingOptions)
    {
        Query.AsNoTracking().ApplyFilters(filter, pricingOptions);
    }
}
