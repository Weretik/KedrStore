using Catalog.Application.Features.Shared;
using Catalog.Application.Shared;
using Catalog.Domain.Entities;

namespace Catalog.Application.Features.Products.Queries.GetProducts;

public static class ProductSortingExtension
{
    public static IOrderedSpecificationBuilder<Product> ApplySorting(
        this ISpecificationBuilder<Product> spec,
        ProductSorter sorter,
        PricingOptions pricingOptions)
    {
        ArgumentNullException.ThrowIfNull(sorter);

        return sorter.Key switch
        {
            ProductSortKey.Name =>
                sorter.Desc
                    ? spec.OrderByDescending(p => p.Name)
                    : spec.OrderBy(p => p.Name),

            ProductSortKey.Category =>
                sorter.Desc
                    ? spec.OrderByDescending(p => p.CategoryId)
                    : spec.OrderBy(p => p.CategoryId),

            ProductSortKey.Stock =>
                sorter.Desc
                    ? spec.OrderByDescending(p => p.Stock)
                    : spec.OrderBy(p => p.Stock),


            _ => spec.OrderBy(p => p.Name)
        };
    }
}
