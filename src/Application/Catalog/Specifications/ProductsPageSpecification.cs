namespace Application.Catalog.Specifications;

public sealed class ProductsPageSpecification : Specification<Product, ProductDto>
{
    public ProductsPageSpecification(ProductsFilter filter, PageRequest pageRequest, string? sort)
    {
        ArgumentNullException.ThrowIfNull(filter);

        Query.AsNoTracking()
            .ApplyCommonFilters(filter)
            .ApplySortingStrict(new ProductSortMap(), sort).ThenBy(p => p.Id)
            .Skip((pageRequest.CurrentPage - 1) * pageRequest.PageSize).Take(pageRequest.PageSize);

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
