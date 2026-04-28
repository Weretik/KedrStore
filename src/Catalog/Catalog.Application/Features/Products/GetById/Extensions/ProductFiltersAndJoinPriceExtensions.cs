using Catalog.Application.Features.Products.GetById.DTOs;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetById.Extensions;

public static class ProductFiltersAndJoinPriceExtensions
{
    public static IQueryable<ProductBySlugDto> JoinPricesAndCategoryForProductBySlug(
        this IQueryable<Product> productsQuery,
        IQueryable<ProductPrice> pricesQuery,
        IQueryable<ProductCategory> categoriesQuery,
        IQueryable<ProductTranslation> translationsQuery,
        GetProductBySlugRequest request)
    {
        var language = NormalizeLanguage(request.Lang);
        translationsQuery = translationsQuery.Where(t => t.Language == language);

        productsQuery = productsQuery.Where(p => p.ProductSlug == request.Slug);

        var withCategoryAndTranslation = productsQuery.LeftJoin(
            translationsQuery,
            product => product.Id,
            translation => translation.ProductId,
            (product, translation) => new { product, translation }
        );

        var withCategory = withCategoryAndTranslation.LeftJoin(
            categoriesQuery,
            query => query.product.CategoryId,
            category => category.Id,
            (query, category) => new { query.product, query.translation, category }
        );

        var priceTypeId = PriceTypeId.From(request.PriceTypeId);
        var filteredPrices  = pricesQuery.Where(pr => pr.PriceTypeId == priceTypeId);

        var withCategoryAndPrice = withCategory.LeftJoin(
            filteredPrices,
            query => query.product.Id,
            productPrice => productPrice.ProductId,
            (query, productPrice) => new { query.product, query.translation, query.category, productPrice }
        );
        return withCategoryAndPrice.Select(query => new ProductBySlugDto
        {
            Id = query.product.Id.Value,
            Name = query.translation != null ? query.translation.Name : query.product.Name,
            Photo = query.product.Photo,
            Scheme = query.product.Sсheme,
            Stock = query.product.Stock,
            CategoryName = query.category!.Name,
            CategorySlug = query.category.Slug,
            QuantityInPack = query.product.QuantityInPack,
            Price = query.productPrice == null ? null : query.productPrice.Amount
        });
    }

    private static string NormalizeLanguage(string? lang)
    {
        if (string.Equals(lang, "ru", StringComparison.OrdinalIgnoreCase))
            return "ru";

        return "uk";
    }
}
