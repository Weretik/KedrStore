using Catalog.Application.Features.Shared;
using Catalog.Application.Shared;
using Catalog.Domain.Entities;

namespace Catalog.Application.Features.Products.Queries.GetProducts;

public sealed class ProductsPageSpec : Specification<Product, ProductDto>
{
    public ProductsPageSpec(
        ProductFilter filter,
        ProductPagination productPagination,
        ProductSorter sorter,
        PricingOptions pricingOptions )
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(productPagination);
        ArgumentNullException.ThrowIfNull(sorter);
        ArgumentNullException.ThrowIfNull(pricingOptions);

        Query.AsNoTracking()
            .ApplyFilters(filter, pricingOptions)
            .ApplySorting(sorter, pricingOptions)
            .ThenBy(p => p.Id);

        if( !productPagination.All)
        {
            Query.Skip((productPagination.CurrentPage - 1) * productPagination.PageSize)
                .Take(productPagination.PageSize);
        }

        Query.Select<Product, ProductDto>(p =>
            new ProductDto(
                Id: p.Id.Value,
                Name: p.Name,
                CategoryId: p.CategoryId.Value,
                Photo: p.Photo,
                Scheme: p.Sсheme,
                Stock: p.Stock,
                IsSale: p.IsSale,
                IsNew: p.IsNew,
                QuantityInPack: p.QuantityInPack
            )
        );
    }
}
