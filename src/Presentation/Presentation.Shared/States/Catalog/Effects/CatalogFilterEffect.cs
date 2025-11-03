namespace Presentation.Shared.States.Catalog.Effects;

public sealed class CatalogFilterEffect(IState<CatalogState> state, ICatalogStore store)
{
    private CancellationTokenSource? _searchDebounceCts;

    [EffectMethod(typeof(CatalogFilterAction.SetFilter))]
    public Task OnSetFilter(IDispatcher dispatcher)
    {
        store.Load();
        return Task.CompletedTask;
    }

    [EffectMethod(typeof(CatalogFilterAction.SetSearchTerm))]
    public async Task OnSetSearchTerm(IDispatcher dispatcher)
    {
        _searchDebounceCts?.Cancel();
        _searchDebounceCts = new CancellationTokenSource();
        var token = _searchDebounceCts.Token;

        try
        {
            await Task.Delay(300, token);

            if (!token.IsCancellationRequested)
                store.Load();
        }
        catch (TaskCanceledException) { }
    }

    [EffectMethod]
    public Task OnSetCategory(CatalogFilterAction.SetCategory action, IDispatcher dispatcher)
    {
        if (action.CategoryId!.Value != state.Value.ProductsFilter.CategoryId!.Value)
            store.Load();

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetStock(CatalogFilterAction.SetStock action, IDispatcher dispatcher)
    {
        if (action.Value != state.Value.ProductsFilter.Stock)
            store.Load();
        return Task.CompletedTask;
    }
}
