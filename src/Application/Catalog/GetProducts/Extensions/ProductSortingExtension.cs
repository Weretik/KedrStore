﻿using Application.Catalog.Shared;
using Domain.Catalog.Entities;

namespace Application.Catalog.GetProducts;

public static class ProductSortingExtension
{
    public static IOrderedSpecificationBuilder<Product> ApplySorting(this ISpecificationBuilder<Product> spec,
        ProductSorter sorter, int priceTypeId)
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

            ProductSortKey.Price =>
                sorter.Desc
                    ? spec.OrderByDescending(p => p.Prices
                        .Where(pp => pp.PriceType == priceTypeId)
                        .Select(pp => pp.Amount)
                        .FirstOrDefault())
                    : spec.OrderBy(p => p.Prices
                        .Where(pp => pp.PriceType == priceTypeId)
                        .Select(pp => pp.Amount)
                        .FirstOrDefault()),

            _ => spec.OrderBy(p => p.Name)
        };
    }
}
