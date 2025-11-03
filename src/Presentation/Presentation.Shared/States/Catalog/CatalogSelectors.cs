using Application.Catalog.GetProducts;
using Application.Catalog.Shared;
// ReSharper disable All

namespace Presentation.Shared.States.Catalog;

public static class CatalogSelectors
{
    // Filter
    public static string GetSearchTerm(CatalogState state) => state.ProductsFilter.SearchTerm ?? "";
    public static int GetCategoryId(CatalogState state) => state.ProductsFilter.CategoryId?.Value ?? 0;
    public static decimal GetStock(CatalogState state) => state.ProductsFilter.Stock ?? 0;

    // Sorting
    public static ProductSortKey GetSortKey(CatalogState state) => state.ProductsSorter.Key;
    public static bool IsSortDescending(CatalogState state) => state.ProductsSorter.Desc;

    // Pagination
    public static int GetCurrentPageNumber(CatalogState state) => state.ProductsPagination.CurrentPage;
    public static int GetPageSize (CatalogState state) => state.ProductsPagination.PageSize;
    public static bool IsAllItemsMode(CatalogState state) => state.ProductsPagination.All;

    // Pricing
    public static string GetPriceTypeId(CatalogState state) => state.PricingOptions.PriceType;
    public static decimal GetMinPrice(CatalogState state) => state.PricingOptions.MinPrice ?? 0;
    public static decimal GetMaxPrice(CatalogState state) => state.PricingOptions.MaxPrice ?? 0;


    public static bool IsLoading(CatalogState state) => state.IsLoading;
    public static string? GetError(CatalogState state) => state.Error;
    public static bool HasError(CatalogState state) => state.Error is not null;


    // ---- GetProductsQueryResult ----
    public static IReadOnlyList<ProductDto> GetItems(CatalogState state) => state.QueryResult?.Items ?? [];
    public static bool IsEmpty(CatalogState state) => state.QueryResult is not null && state.QueryResult.TotalItems == 0;
    public static int GetTotalItems(CatalogState state) => state.QueryResult?.TotalItems ?? 0;
    public static int GetTotalPages(CatalogState state) => state.QueryResult?.TotalPages ?? 0;

    public static decimal GetPriceForType(CatalogState state, string priceType)
    {
        if (!IsEmpty(state))
        {
            var items = state.QueryResult!.Items;
            var amount = items
                .SelectMany(p => p.Prices)
                .FirstOrDefault(price => price.PriceType == priceType)?.Amount;

            return amount.GetValueOrDefault();
        }
        return 0;
    }
}
