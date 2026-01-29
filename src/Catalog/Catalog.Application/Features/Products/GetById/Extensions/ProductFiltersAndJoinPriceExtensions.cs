using Catalog.Application.Features.Products.GetById.DTOs;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetById.Extensions;

public static class ProductFiltersAndJoinPriceExtensions
{
    public static IQueryable<ProductBySlugDto> JoinPricesAndCategoryForProductBySlug(
        this IQueryable<Product> productsQuery,
        IQueryable<ProductPrice> pricesQuery,
        IQueryable<ProductCategory> categoriesQuery,
        GetProductBySlugRequest request)
    {
        productsQuery = productsQuery.Where(p => p.ProductSlug == request.Slug);

        var withCategory = productsQuery.LeftJoin(
            categoriesQuery,
            product => product.CategoryId,
            category => category.Id,
            (product, category) => new { product, category }
        );

        var priceTypeId = PriceTypeId.From(request.PriceTypeId);
        var filteredPrices  = pricesQuery.Where(pr => pr.PriceTypeId == priceTypeId);

        var withCategoryAndPrice = withCategory.LeftJoin(
            filteredPrices,
            query => query.product.Id,
            productPrice => productPrice.ProductId,
            (query, productPrice) => new { query.product, query.category, productPrice }
        );
        return withCategoryAndPrice.Select(query => new ProductBySlugDto
        {
            Id = query.product.Id.Value,
            Name = query.product.Name,
            Photo = query.product.Photo,
            Scheme = query.product.Sсheme,
            Stock = query.product.Stock,
            CategoryName = query.category!.Name,
            CategorySlug = query.category.Slug,
            QuantityInPack = query.product.QuantityInPack,
            Price = query.productPrice == null ? null : query.productPrice.Amount
        });
    }

}
