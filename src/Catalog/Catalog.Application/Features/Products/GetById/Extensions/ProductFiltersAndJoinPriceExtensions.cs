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
            p => p.CategoryId,
            c => c.Id,
            (p, c) => new { p, c }
        );

        var priceTypeId = PriceTypeId.From(request.PriceTypeId);
        var filteredPrices  = pricesQuery.Where(pr => pr.PriceTypeId == priceTypeId);

        var withCategoryAndPrice = withCategory.LeftJoin(
            filteredPrices,
            x => x.p.Id,
            pr => pr.ProductId,
            (x, pr) => new { x.p, x.c, pr }
        );
        return withCategoryAndPrice.Select(x => new ProductBySlugDto
        {
            Id = x.p.Id.Value,
            Name = x.p.Name,
            Photo = x.p.Photo,
            Scheme = x.p.Sсheme,
            Stock = x.p.Stock,
            CategoryName = x.c.Name,
            CategorySlug = x.c.Slug,
            Price = x.pr!.Amount
        });
    }

}
