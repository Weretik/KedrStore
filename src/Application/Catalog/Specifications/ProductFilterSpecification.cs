namespace Domain.Catalog.Specifications;

public class ProductFilterSpecification(
    string? searchTerm = null,
    decimal? minPrice = null,
    decimal? maxPrice = null,
    CategoryId? categoryId = null,
    string? manufacturer = null)
    : SpecificationBase<Product>
{
    private readonly SearchTermSpecification _searchSpec = new(searchTerm);
    private readonly PriceRangeSpecification _priceSpec = new(minPrice, maxPrice);
    private readonly CategorySpecification _categorySpec = new(categoryId);
    private readonly ManufacturerSpecification _manufacturerSpec = new(manufacturer);

    public override Expression<Func<Product, bool>> ToExpression()
    {
        return _searchSpec
            .And(_priceSpec)
            .And(_categorySpec)
            .And(_manufacturerSpec)
            .ToExpression();
    }

}
