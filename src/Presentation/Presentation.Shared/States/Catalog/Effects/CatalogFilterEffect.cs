namespace Presentation.Shared.States.Catalog.Effects;

public sealed class CatalogFilterEffect(IState<CatalogState> state, ICatalogStore store)
{
    private CancellationTokenSource? _searchDebounceCts;

    [EffectMethod(typeof(CatalogFilterAction.SetFilter))]
    public Task OnSetFilter()
    {
        store.Load();
        return Task.CompletedTask;
    }

    [EffectMethod(typeof(CatalogFilterAction.SetSearchTerm))]
    public async Task OnSetSearchTerm()
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
    public Task OnSetCategory(CatalogFilterAction.SetCategory action)
    {
        if (action.CategoryId != state.Value.ProductsFilter.CategoryId)
            store.Load();

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetStock(CatalogFilterAction.SetStock action)
    {
        if (action.Value != state.Value.ProductsFilter.Stock)
            store.Load();
        return Task.CompletedTask;
    }
}
