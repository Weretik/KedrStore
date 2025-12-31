using Catalog.Application.Features.Shared;
using Catalog.Application.Shared;

namespace Presentation.Shared.States.Catalog;

public static class CatalogSelectors
{
    // Filter
    public static string GetSearchTerm(this CatalogState state) => state.ProductsFilter.SearchTerm ?? "";
    public static int GetCategoryId(this CatalogState state) => state.ProductsFilter.CategoryId ?? 0;
    public static decimal GetStock(this CatalogState state) => state.ProductsFilter.Stock ?? 0;

    // Sorting
    public static ProductSortKey GetSortKey(this CatalogState state) => state.ProductsSorter.Key;
    public static bool IsSortDescending(this CatalogState state) => state.ProductsSorter.Desc;

    // Pagination
    public static int GetCurrentPageNumber(this CatalogState state) => state.ProductsPagination.CurrentPage;
    public static int GetPageSize (this CatalogState state) => state.ProductsPagination.PageSize;
    public static bool IsAllItemsMode(this CatalogState state) => state.ProductsPagination.All;

    // Pricing
    public static string GetPriceTypeId(this CatalogState state) => state.PricingOptions.PriceType;
    public static decimal GetMinPrice(this CatalogState state) => state.PricingOptions.MinPrice ?? 0;
    public static decimal GetMaxPrice(this CatalogState state) => state.PricingOptions.MaxPrice ?? 0;


    public static bool IsLoading(this CatalogState state) => state.IsLoading;
    public static string? GetError(this CatalogState state) => state.Error;
    public static bool HasError(this CatalogState state) => state.Error is not null;


    // ---- GetProductsQueryResult ----
    public static IReadOnlyList<ProductDto> GetItems(this CatalogState state) => state.QueryResult?.Items ?? [];
    public static bool IsEmpty(this CatalogState state) => state.QueryResult is not null && state.QueryResult.TotalItems == 0;
    public static int GetTotalItems(this CatalogState state) => state.QueryResult?.TotalItems ?? 0;
    public static int GetTotalPages(this CatalogState state) => state.QueryResult?.TotalPages ?? 0;
}
