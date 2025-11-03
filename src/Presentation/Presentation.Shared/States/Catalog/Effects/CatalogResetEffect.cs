namespace Presentation.Shared.States.Catalog.Effects;

public sealed class CatalogResetEffect
{
    [EffectMethod(typeof(CatalogResetAction.Reset))]
    public Task OnReset(IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new CatalogLoadAction.Load());
        return Task.CompletedTask;
    }
}
