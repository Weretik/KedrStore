namespace Presentation.Shared.States.Catalog.Effects;

public sealed class CatalogSortingEffect(IState<CatalogState> state)
{
    [EffectMethod]
    public Task OnSetSorter(CatalogSortingAction.SetSorter action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new CatalogLoadAction.Load());
        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetSortKey(CatalogSortingAction.SetSortKey action, IDispatcher dispatcher)
    {
        if (action.Key != state.Value.ProductsSorter.Key)
            dispatcher.Dispatch(new CatalogLoadAction.Load());

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetSortDesc(CatalogSortingAction.SetSortDesc action, IDispatcher dispatcher)
    {
        if (action.Desc != state.Value.ProductsSorter.Desc)
            dispatcher.Dispatch(new CatalogLoadAction.Load());

        return Task.CompletedTask;
    }
}
