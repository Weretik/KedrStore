using Presentation.Shared.Extensions;

namespace Presentation.Shared.States.Catalog;

public static class CatalogFilterReducer
{
    [ReducerMethod]
    public static CatalogState OnSetFilter(CatalogState state, CatalogFilterAction.SetFilter action)
        =>(state with { ProductsFilter = action.Filter }).ResetPage();

    [ReducerMethod]
    public static CatalogState OnSetSearchTerm(CatalogState state, CatalogFilterAction.SetSearchTerm action)
    {
        if (action.SearchTerm == state.ProductsFilter.SearchTerm) return state;

        var updatedState = state with
        {
            ProductsFilter = state.ProductsFilter with
            {
                SearchTerm = action.SearchTerm.NormalizeOrNull()
            }
        };

        return updatedState.ResetPage();
    }

    [ReducerMethod]
    public static CatalogState OnSetCategory(CatalogState state, CatalogFilterAction.SetCategory action)
    {
        if (action.CategoryId!.Value == state.ProductsFilter.CategoryId!.Value) return state;

        var updatedState = state with
        {
            ProductsFilter = state.ProductsFilter with
            {
                CategoryId = action.CategoryId
            }
        };
        return updatedState.ResetPage();
    }


    [ReducerMethod]
    public static CatalogState OnSetStock(CatalogState state, CatalogFilterAction.SetStock action)
    {
        if (action.Stock == state.ProductsFilter.CategoryId!.Value) return state;

        var updatedState = state with
        {
            ProductsFilter = state.ProductsFilter with { Stock = action.Stock }
        };

        return updatedState.ResetPage();
    }
}
