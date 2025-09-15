namespace Application.Catalog.Specifications;

public sealed class ProductsPageSpecification : Specification<Product, ProductDto>
{
    public ProductsPageSpecification(ProductsCriteria criteria)
    {
        Query.AsNoTracking()
            .ApplyCommonFilters(criteria)
            .ApplySortingStrict(new ProductSortMap(), criteria.Sort).ThenBy(p => p.Id)
            .Skip((criteria.PageNumber - 1) * criteria.PageSize).Take(criteria.PageSize);

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
