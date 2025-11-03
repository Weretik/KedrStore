namespace Presentation.Shared.States.Catalog.Effects;

public sealed class CatalogPaginationEffect(IState<CatalogState> state, ICatalogStore store)
{
    [EffectMethod(typeof(CatalogPaginationAction.SetPagination))]
    public Task OnSetPagination(IDispatcher dispatcher)
    {
        store.Load();
        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetPageNumber(CatalogPaginationAction.SetPageNumber action, IDispatcher dispatcher)
    {
        if (action.PageNumber != state.Value.ProductsPagination.CurrentPage)
            store.Load();

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetPageSize(CatalogPaginationAction.SetPageSize action, IDispatcher dispatcher)
    {
        if (action.PageSize != state.Value.ProductsPagination.PageSize)
            store.Load();

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetAllPage(CatalogPaginationAction.SetAllPageSize action, IDispatcher dispatcher)
    {
        if (action.All != state.Value.ProductsPagination.All)
            store.Load();

        return Task.CompletedTask;
    }
}
