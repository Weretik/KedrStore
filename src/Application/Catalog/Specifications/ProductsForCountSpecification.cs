namespace Application.Catalog.Specifications;

public class ProductsForCountSpecification : Specification<Product>
{
    public ProductsForCountSpecification(
        string? search,
        CategoryId? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        string? manufacturer)
    {
        Query.AsNoTracking()
            .ApplyCommonFilters(search, categoryId, minPrice, maxPrice, manufacturer);
    }
}
