using Catalog.Application.Features.Products.GetList.DTOs;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetList.Extensions;

public static class ProductListJoinExtensions
{
    public static IQueryable<ProductListRowDto> JoinPricesForList(
        this IQueryable<Product> productsQuery,
        IQueryable<ProductPrice> pricesQuery,
        IQueryable<ProductTranslation> translationsQuery,
        GetProductsRequest request,
        string hardwareRootCategoryId)
    {
        var language = NormalizeLanguage(request.Lang);
        translationsQuery = translationsQuery.Where(t => t.Language == language);

        var productsWithTranslations = productsQuery.LeftJoin(
            translationsQuery,
            product => product.Id,
            translation => translation.ProductId,
            (product, translation) => new { product, translation });

        pricesQuery = pricesQuery.Where(pr => pr.PriceTypeId == PriceTypeId.From(request.PriceTypeId));

        var productListQuery =
            productsWithTranslations.LeftJoin(pricesQuery,
                row => row.product.Id,
                pr => pr.ProductId,
                (row, price) => new ProductListRowDto
                {
                    Id = row.product.Id.Value,
                    Name = row.translation != null ? row.translation.Name : row.product.Name,
                    Photo = row.product.Photo,
                    ProductSlug = row.product.ProductSlug,
                    CategoryId = row.product.CategoryId.Value,
                    InStock = row.product.ProductTypeIdOneC == hardwareRootCategoryId
                        ? row.product.Stock > 2
                        : row.product.Stock > 0,
                    IsSale = row.product.IsSale,
                    IsNew = row.product.IsNew,
                    Price = price != null ? price.Amount : null

                });

        if (request.PriceFrom is not null)
            productListQuery = productListQuery
                .Where(x => x.Price != null && x.Price >= request.PriceFrom.Value);

        if (request.PriceTo is not null)
            productListQuery = productListQuery
                .Where(x => x.Price != null && x.Price <= request.PriceTo.Value);


        return productListQuery;
    }

    private static string NormalizeLanguage(string? lang)
    {
        if (string.Equals(lang, "ru", StringComparison.OrdinalIgnoreCase))
            return "ru";

        return "uk";
    }
}
