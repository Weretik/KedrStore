namespace Presentation.Shared.States.Catalog;

public static class CatalogPaginationReducer
{
    [ReducerMethod]
    public static CatalogState OnSetPagination(CatalogState state, CatalogPaginationAction.SetPagination action)
        => state with { ProductsPagination = action.Pagination };

    [ReducerMethod]
    public static CatalogState OnSetPageNumber(CatalogState state, CatalogPaginationAction.SetPageNumber action)
    {
        if (action.PageNumber != state.ProductsPagination.CurrentPage) return state;

        var updateState = state with
        {
            ProductsPagination = state.ProductsPagination with { CurrentPage = action.PageNumber }
        };
        return updateState;
    }

    [ReducerMethod]
    public static CatalogState OnSetPageSize(CatalogState state, CatalogPaginationAction.SetPageSize action)
    {
        if (action.PageSize != state.ProductsPagination.PageSize) return state;

        var updateState = state with
        {
            ProductsPagination = state.ProductsPagination with { PageSize = action.PageSize, CurrentPage = 1 }
        };
        return updateState;
    }


    [ReducerMethod]
    public static CatalogState OnSetAllPage(CatalogState state, CatalogPaginationAction.SetAllPageSize action)
    {
        if (action.All != state.ProductsPagination.All) return state;

        var updateState = state with
        {
            ProductsPagination = state.ProductsPagination with { All = action.All }
        };
        return updateState;
    }

}
