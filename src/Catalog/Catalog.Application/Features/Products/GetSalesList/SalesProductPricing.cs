using Catalog.Application.Querying;
using Catalog.Contracts.Pricing;
using Catalog.Contracts.Products.GetList;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetSalesList;

/// <summary>
/// Joins products with the customer's prices: a category rule picks the price type for products of that category,
/// all other products use the default price type (the customer's or retail).
/// </summary>
internal static class SalesProductPricing
{
    public static IQueryable<SalesProductRow> JoinPrices(
        IQueryable<Product> productsQuery,
        IQueryable<ProductPrice> pricesQuery,
        IQueryable<ProductTranslation> translationsQuery,
        GetProductsRequest request,
        int retailPriceTypeId,
        string hardwareRootCategoryId)
    {
        var language = CatalogLanguage.Normalize(request.Lang);
        var localizedTranslations = translationsQuery.Where(t => t.Language == language);
        var defaultPriceTypeId = PriceTypeId.From(request.PriceTypeId ?? retailPriceTypeId);
        var priceTypeRules = NormalizeRules(request.PriceTypeRules);

        IQueryable<SalesProductRow>? rows = null;

        foreach (var rule in priceTypeRules)
        {
            var categoryId = ProductCategoryId.From(rule.CategoryId);
            var categoryRows = JoinWithPriceType(
                productsQuery.Where(product => product.CategoryId == categoryId),
                pricesQuery,
                localizedTranslations,
                PriceTypeId.From(rule.PriceTypeId),
                hardwareRootCategoryId);

            rows = rows is null ? categoryRows : rows.Concat(categoryRows);
        }

        var ruleCategoryIds = priceTypeRules.Select(rule => ProductCategoryId.From(rule.CategoryId)).ToArray();
        var defaultProducts = ruleCategoryIds.Length == 0
            ? productsQuery
            : productsQuery.Where(product => !ruleCategoryIds.Contains(product.CategoryId));

        var defaultRows = JoinWithPriceType(
            defaultProducts, pricesQuery, localizedTranslations, defaultPriceTypeId, hardwareRootCategoryId);

        rows = rows is null ? defaultRows : rows.Concat(defaultRows);

        if (request.PriceFrom is not null)
            rows = rows.Where(x => x.Price != null && x.Price >= request.PriceFrom.Value);

        if (request.PriceTo is not null)
            rows = rows.Where(x => x.Price != null && x.Price <= request.PriceTo.Value);

        return rows;
    }

    private static IQueryable<SalesProductRow> JoinWithPriceType(
        IQueryable<Product> productsQuery,
        IQueryable<ProductPrice> pricesQuery,
        IQueryable<ProductTranslation> translationsQuery,
        PriceTypeId priceTypeId,
        string hardwareRootCategoryId)
    {
        var pricesByType = pricesQuery.Where(price => price.PriceTypeId == priceTypeId);

        var productsWithTranslations = productsQuery.LeftJoin(
            translationsQuery,
            product => product.Id,
            translation => translation.ProductId,
            (product, translation) => new { product, translation });

        return productsWithTranslations.LeftJoin(
            pricesByType,
            row => row.product.Id,
            price => price.ProductId,
            (row, price) => new SalesProductRow
            {
                Id = row.product.Id,
                Name = row.translation != null ? row.translation.Name : row.product.Name,
                Photo = row.product.Photo,
                ProductSlug = row.product.ProductSlug,
                CategoryId = row.product.CategoryId,
                InStock = row.product.ProductTypeIdOneC == hardwareRootCategoryId
                    ? row.product.Stock > 2
                    : row.product.Stock > 0,
                IsSale = row.product.IsSale,
                IsNew = row.product.IsNew,
                Price = price != null ? price.Amount : null
            });
    }

    /// <summary>Drops invalid rules and keeps the first rule of each category.</summary>
    private static IReadOnlyCollection<CategoryPriceTypeRule> NormalizeRules(
        IReadOnlyCollection<CategoryPriceTypeRule> priceTypeRules)
        => priceTypeRules
            .Where(rule => rule.CategoryId > 0 && rule.PriceTypeId > 0)
            .GroupBy(rule => rule.CategoryId)
            .Select(group => group.First())
            .ToArray();
}
