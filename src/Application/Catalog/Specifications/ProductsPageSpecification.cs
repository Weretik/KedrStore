namespace Application.Catalog.Specifications;

public sealed class ProductsPageSpecification : Specification<Product, ProductDto>
{
    public ProductsPageSpecification(
        string? search,
        CategoryId? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        string? manufacturer,
        string? sort,
        int page,
        int pageSize)
    {
        Query.AsNoTracking()
            .ApplyCommonFilters(search, categoryId, minPrice, maxPrice, manufacturer)
            .ApplySortingStrict(new ProductSortMap(), sort).ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize).Take(pageSize);

        Query.Select(p => new ProductDto
        {
            Id           = p.Id.Value,
            Name         = p.Name,
            Manufacturer = p.Manufacturer,
            Amount       = p.Price.Amount,
            Currency     = p.Price.Currency,
            CategoryId   = p.CategoryId.Value,
            Photo        = p.Photo
        });
    }
}
