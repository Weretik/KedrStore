using Application.Catalog.GetCategories;

namespace Presentation.Shared.States.Category;

public static class CategoryReduce
{
    [ReducerMethod]
    public static CategoryState OnSetFilter(CategoryState state, CategoryAction.SetFilter action)
        => state with { Filter = action.Filter };

    [ReducerMethod]
    public static CategoryState OnSetProductTypeId(CategoryState state, CategoryAction.SetProductTypeId action)
    {
        if (action.ProductTypeId == state.Filter.ProductTypeId) return state;

        return state with
        {
            Filter = new CategoryFilter(ProductTypeId: action.ProductTypeId)
        };
    }

    [ReducerMethod(typeof(CategoryAction.Load))]
    public static CategoryState OnLoad(CategoryState state)
        => state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static CategoryState OnLoadSuccess(CategoryState state, CategoryAction.LoadSuccess action)
        => state with { IsLoading = false, Error = null, QueryResult = action.QueryResult };

    [ReducerMethod]
    public static CategoryState OnLoadFailure(CategoryState state, CategoryAction.LoadFailure action)
        => state with { IsLoading = false, Error = action.Error };
}
