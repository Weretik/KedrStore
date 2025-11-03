namespace Presentation.Shared.States.Catalog.Effects;

public sealed class CatalogPaginationEffect(IState<CatalogState> state)
{
    [EffectMethod]
    public Task OnSetPagination(CatalogPaginationAction.SetPagination action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new CatalogLoadAction.Load());
        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetPageNumber(CatalogPaginationAction.SetPageNumber action, IDispatcher dispatcher)
    {
        if (action.PageNumber != state.Value.ProductsPagination.CurrentPage)
            dispatcher.Dispatch(new CatalogLoadAction.Load());

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetPageSize(CatalogPaginationAction.SetPageSize action, IDispatcher dispatcher)
    {
        if (action.PageSize != state.Value.ProductsPagination.PageSize)
            dispatcher.Dispatch(new CatalogLoadAction.Load());

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetAllPage(CatalogPaginationAction.SetAllPage action, IDispatcher dispatcher)
    {
        if (action.All != state.Value.ProductsPagination.All)
            dispatcher.Dispatch(new CatalogLoadAction.Load());

        return Task.CompletedTask;
    }
}
