using Application.Catalog.Shared;
using Domain.Catalog.Entities;

namespace Application.Catalog.GetProducts;

public sealed class ProductsPageSpec : Specification<Product, ProductDto>
{
    public ProductsPageSpec(ProductFilter filter, ProductPagination productPagination, ProductSorter sorter, int priceTypeId)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(productPagination);

        Query.AsNoTracking()
            .ApplyFilters(filter, priceTypeId)
            .ApplySorting(sorter, priceTypeId)
            .ThenBy(p => p.Id)
            .Skip((productPagination.CurrentPage - 1) * productPagination.PageSize)
            .Take(productPagination.PageSize);

        Query.Select<Product, ProductDto>(p => new ProductDto(
                p.Id.Value,
                p.Name,
                p.CategoryId.Value,
                p.ProductType.Value,
                p.Photo,
                p.Stock,
                p.Prices.Select(price => new ProductPriceDto(
                        price.PriceType.Name,
                        price.Price.Amount,
                        price.Price.Currency.Code)
                ).ToList())
        );
    }
}
