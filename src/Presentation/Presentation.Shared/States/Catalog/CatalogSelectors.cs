using Application.Catalog.GetProducts;
using Application.Catalog.Shared;

namespace Presentation.Shared.States.Catalog;

public static class CatalogSelectors
{
    public static ProductFilter Filter(CatalogState state) => state.ProductsFilter;
    public static ProductSorter Sorter(CatalogState state) => state.ProductsSorter;
    public static ProductPagination Pagination(CatalogState state) => state.ProductsPagination;
    public static PricingOptions Pricing(CatalogState state) => state.PricingOptions;
    public static bool IsLoading(CatalogState state) => state.IsLoading;
    public static string? Error(CatalogState state) => state.Error;

    // ---- GetProductsQueryResult ----
    public static IReadOnlyList<ProductDto> Items(CatalogState state) => state.QueryResult?.Items ?? [];
    public static int TotalItems(CatalogState state) => state.QueryResult?.TotalItems ?? 0;
    public static int TotalPages(CatalogState state) => state.QueryResult?.TotalPages ?? 0;
    public static bool HasError(CatalogState state) => state.Error is not null;
    public static bool IsEmpty(CatalogState state) => state.QueryResult is not null && state.QueryResult.TotalItems == 0;
}
