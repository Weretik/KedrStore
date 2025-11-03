namespace Presentation.Shared.States.Catalog;

public static class CatalogResetReducer
{
    [ReducerMethod(typeof(CatalogResetAction.Reset))]
    public static CatalogState OnReset(CatalogState state) => new();
}
