namespace Presentation.Shared.States.Catalog.Effects;

public sealed class CatalogFilterEffect(IState<CatalogState> state, ICatalogStore store)
{
    private CancellationTokenSource? _сancellationToken;

    [EffectMethod(typeof(CatalogFilterAction.SetFilter))]
    public Task OnSetFilter(IDispatcher dispatcher)
    {
        store.Load();
        return Task.CompletedTask;
    }

    [EffectMethod(typeof(CatalogFilterAction.SetSearchTerm))]
    public async Task OnSetSearchTerm(IDispatcher dispatcher)
    {
        _сancellationToken?.Cancel();
        _сancellationToken = new CancellationTokenSource();
        var token = _сancellationToken.Token;

        try
        {
            await Task.Delay(450, token);

            if (!token.IsCancellationRequested)
                store.Load();
        }
        catch (TaskCanceledException) { }
    }

    [EffectMethod]
    public Task OnSetCategory(CatalogFilterAction.SetCategory action, IDispatcher dispatcher)
    {
        store.Load();
        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetStock(CatalogFilterAction.SetStock action, IDispatcher dispatcher)
    {
        store.Load();
        return Task.CompletedTask;
    }
}
