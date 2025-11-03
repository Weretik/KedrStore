namespace Presentation.Shared.States.Catalog;

public static class CatalogPaginationReducer
{
    [ReducerMethod]
    public static CatalogState OnSetPagination(CatalogState state, CatalogPaginationAction.SetPagination action)
        => state with { ProductsPagination = action.Pagination };

    [ReducerMethod]
    public static CatalogState OnSetPageNumber(CatalogState state, CatalogPaginationAction.SetPageNumber action)
        => state with
        {
            ProductsPagination = state.ProductsPagination with { CurrentPage = action.PageNumber }
        };

    [ReducerMethod]
    public static CatalogState OnSetPageSize(CatalogState state, CatalogPaginationAction.SetPageSize action)
        => state with
        {
            ProductsPagination = state.ProductsPagination with { PageSize = action.PageSize, CurrentPage = 1 }
        };

    [ReducerMethod]
    public static CatalogState OnSetAllPage(CatalogState state, CatalogPaginationAction.SetAllPageSize action)
        => state with
        {
            ProductsPagination = state.ProductsPagination with { All = action.All }
        };
}
