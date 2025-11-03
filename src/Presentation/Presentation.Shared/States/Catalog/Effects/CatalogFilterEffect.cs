namespace Presentation.Shared.States.Catalog.Effects;

public sealed class CatalogFilterEffect(IState<CatalogState> state)
{
    private CancellationTokenSource? _searchDebounceCts;

    [EffectMethod]
    public Task OnSetFilter(CatalogFilterAction.SetFilter action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new CatalogLoadAction.Load());
        return Task.CompletedTask;
    }

    [EffectMethod]
    public async Task OnSetSearchTerm(CatalogFilterAction.SetSearchTerm action, IDispatcher dispatcher)
    {
        _searchDebounceCts?.Cancel();
        _searchDebounceCts = new CancellationTokenSource();
        var token = _searchDebounceCts.Token;

        try
        {
            await Task.Delay(300, token);

            if (!token.IsCancellationRequested)
                dispatcher.Dispatch(new CatalogLoadAction.Load());
        }
        catch (TaskCanceledException) { }
    }

    [EffectMethod]
    public Task OnSetCategory(CatalogFilterAction.SetCategory action, IDispatcher dispatcher)
    {
        if (action.CategoryId != state.Value.ProductsFilter.CategoryId)
            dispatcher.Dispatch(new CatalogLoadAction.Load());
        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetStock(CatalogFilterAction.SetStock action, IDispatcher dispatcher)
    {
        if (action.Value != state.Value.ProductsFilter.Stock)
            dispatcher.Dispatch(new CatalogLoadAction.Load());
        return Task.CompletedTask;
    }
}
