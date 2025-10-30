using Domain.Catalog.Entities;

namespace Application.Catalog.GetProducts;

public class ProductsForCountSpec : Specification<Product>
{
    public ProductsForCountSpec(ProductFilter filter, int priceTypeId)
    {
        Query.AsNoTracking().ApplyFilters(filter, priceTypeId);
    }
}
