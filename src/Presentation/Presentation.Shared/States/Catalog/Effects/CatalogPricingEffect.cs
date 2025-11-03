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
        var min = state.Value.PricingOptions.MinPrice;
        var max = state.Value.PricingOptions.MaxPrice;
        var hasNotChanged  = action.MinPrice == min && action.MaxPrice == max;

        if (!hasNotChanged)
            store.Load();

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetPrice(CatalogPricingAction.SetPriceId action, IDispatcher dispatcher)
    {
        if (action.PriceType != state.Value.PricingOptions.PriceType)
            store.Load();

        return Task.CompletedTask;
    }
}
