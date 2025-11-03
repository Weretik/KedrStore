using Application.Catalog.GetProducts;
using Presentation.Shared.Extensions;

namespace Presentation.Shared.States.Catalog;

public static class CatalogSortingReducer
{
    [ReducerMethod]
    public static CatalogState OnSetSorter(CatalogState state, CatalogSortingAction.SetSorter action)
        => (state with { ProductsSorter = action.Sorter, }).ResetPage();

    [ReducerMethod]
    public static CatalogState OnSetSortKey(CatalogState state, CatalogSortingAction.SetSortKey action)
    {
        var current = state.ProductsSorter;
        var isSameKey = current.Key == action.Key;

        var newSorter = isSameKey
            ? current
            : new ProductSorter(Key: action.Key, Desc: false);

        var updateState = state with
        {
            ProductsSorter = newSorter,
            ProductsPagination = state.ProductsPagination with { CurrentPage = 1 },
        };

        return updateState.ResetPage();
    }

    [ReducerMethod]
    public static CatalogState OnSetSortDesc(CatalogState state, CatalogSortingAction.SetSortDesc action)
    {
        var updateState = state with
        {
            ProductsSorter = state.ProductsSorter with { Desc = action.Desc },

        };
        return updateState.ResetPage();
    }
}
