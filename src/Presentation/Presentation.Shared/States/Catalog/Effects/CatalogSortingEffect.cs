namespace Presentation.Shared.States.Catalog.Effects;

public sealed class CatalogSortingEffect(IState<CatalogState> state, ICatalogStore store)
{
    [EffectMethod]
    public Task OnSetSorter(CatalogSortingAction.SetSorter action)
    {
        store.Load();
        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetSortKey(CatalogSortingAction.SetSortKey action)
    {
        if (action.Key != state.Value.ProductsSorter.Key)
            store.Load();

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetSortDesc(CatalogSortingAction.SetSortDesc action)
    {
        if (action.Desc != state.Value.ProductsSorter.Desc)
            store.Load();

        return Task.CompletedTask;
    }
}
