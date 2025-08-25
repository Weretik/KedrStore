namespace Presentation.Shared.States.Catalog;

public static class CatalogReducers
{
    [ReducerMethod]
    public static CatalogState OnSetParams(CatalogState state, CatalogActions.SetParams action)
        => state with { Params = action.Params };

    [ReducerMethod]
    public static CatalogState OnResetParams(CatalogState state, CatalogActions.ResetParams _)
        => state with { Params = new CatalogParams() };

    [ReducerMethod]
    public static CatalogState OnSetPageNumber(CatalogState state, CatalogActions.SetPageNumber action)
        => state with
        {
            Params = state.Params with
            {
                PageNumber = action.PageNumber <= 0 ? 1 : action.PageNumber
            }
        };

    [ReducerMethod]
    public static CatalogState OnLoad(CatalogState state, CatalogActions.Load _)
        => state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static CatalogState OnLoadSuccess(CatalogState state, CatalogActions.LoadSuccess action)
        => state with { IsLoading = false, Error = null, PageList = action.PageList };

    [ReducerMethod]
    public static CatalogState OnLoadFailure(CatalogState state, CatalogActions.LoadFailure action)
        => state with { IsLoading = false, Error = action.Error };
}
