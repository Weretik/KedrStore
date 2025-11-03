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
        var updatedState = state with
        {
            ProductsFilter = state.ProductsFilter with { SearchTerm = action.Value.NormalizeOrNull() }
        };

        return updatedState.ResetPage();
    }

    [ReducerMethod]
    public static CatalogState OnSetCategory(CatalogState state, CatalogFilterAction.SetCategory action)
    {
        var updatedState = state with
        {
            ProductsFilter = state.ProductsFilter with { CategoryId = action.CategoryId }
        };

        return updatedState.ResetPage();
    }


    [ReducerMethod]
    public static CatalogState OnSetStock(CatalogState state, CatalogFilterAction.SetStock action)
    {
        var updatedState = state with
        {
            ProductsFilter = state.ProductsFilter with { Stock = action.Value }
        };

        return updatedState.ResetPage();
    }
}
