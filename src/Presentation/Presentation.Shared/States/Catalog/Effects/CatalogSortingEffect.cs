namespace Presentation.Shared.States.Catalog.Effects;

public sealed class CatalogSortingEffect(IState<CatalogState> state, ICatalogStore store)
{
    [EffectMethod]
    public Task OnSetSorter(CatalogSortingAction.SetSorter action, IDispatcher dispatcher)
    {
        store.Load();
        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetSortKey(CatalogSortingAction.SetSortKey action, IDispatcher dispatcher)
    {
        if (action.Key != state.Value.ProductsSorter.Key)
            store.Load();

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetSortDesc(CatalogSortingAction.SetSortDesc action, IDispatcher dispatcher)
    {
        if (action.Desc != state.Value.ProductsSorter.Desc)
            store.Load();

        return Task.CompletedTask;
    }
}
