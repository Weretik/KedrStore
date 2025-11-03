namespace Presentation.Shared.States.Catalog;

public static class CatalogLoadReducer
{
    [ReducerMethod(typeof(CatalogLoadAction.Load))]
    public static CatalogState OnLoad(CatalogState state)
        => state with { IsLoading = true, Error = null };

    [ReducerMethod(typeof(CatalogLoadAction.Reset))]
    public static CatalogState OnReset(CatalogState state) => new();

    [ReducerMethod]
    public static CatalogState OnLoadSuccess(CatalogState state, CatalogLoadAction.LoadSuccess action)
        => state with { IsLoading = false, Error = null, QueryResult = action.QueryResult };

    [ReducerMethod]
    public static CatalogState OnLoadFailure(CatalogState state, CatalogLoadAction.LoadFailure action)
        => state with { IsLoading = false, Error = action.Error };
}
