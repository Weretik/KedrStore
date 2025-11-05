namespace Presentation.Shared.States.Catalog.Effects;


public sealed class CatalogPricingEffects(IState<CatalogState> state, ICatalogStore store)
{
    [EffectMethod(typeof(CatalogPricingAction.SetPricingOptions))]
    public Task OnSetPricingOptions(IDispatcher dispatcher)
    {
        store.Load();
        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetPriceRange(CatalogPricingAction.SetPriceRange action, IDispatcher dispatcher)
    {
        store.Load();
        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetPrice(CatalogPricingAction.SetPriceType action, IDispatcher dispatcher)
    {
        store.Load();
        return Task.CompletedTask;
    }
}
