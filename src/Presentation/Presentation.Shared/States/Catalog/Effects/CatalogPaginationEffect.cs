namespace Presentation.Shared.States.Catalog.Effects;

public sealed class CatalogPaginationEffect(IState<CatalogState> state, ICatalogStore store)
{
    [EffectMethod(typeof(CatalogPaginationAction.SetPagination))]
    public Task OnSetPagination()
    {
        store.Load();
        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetPageNumber(CatalogPaginationAction.SetPageNumber action)
    {
        if (action.PageNumber != state.Value.ProductsPagination.CurrentPage)
            store.Load();

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetPageSize(CatalogPaginationAction.SetPageSize action)
    {
        if (action.PageSize != state.Value.ProductsPagination.PageSize)
            store.Load();

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetAllPage(CatalogPaginationAction.SetAllPageSize action)
    {
        if (action.All != state.Value.ProductsPagination.All)
            store.Load();

        return Task.CompletedTask;
    }
}
