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
        store.Load();
        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetPageSize(CatalogPaginationAction.SetPageSize action, IDispatcher dispatcher)
    {
        store.Load();
        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetAllPage(CatalogPaginationAction.SetAllPageSize action, IDispatcher dispatcher)
    {
        store.Load();
        return Task.CompletedTask;
    }
}
