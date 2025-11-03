namespace Presentation.Shared.States.Catalog.Effects;


public sealed class CatalogPricingEffects(IState<CatalogState> state)
{
    [EffectMethod]
    public Task OnSetPricingOptions(CatalogPricingAction.SetPricingOptions action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new CatalogLoadAction.Load());
        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetPriceRange(CatalogPricingAction.SetPriceRange action, IDispatcher dispatcher)
    {
        var min = state.Value.PricingOptions.MinPrice;
        var max = state.Value.PricingOptions.MaxPrice;
        var hasNotChanged  = action.MinPrice == min && action.MaxPrice == max;

        if (!hasNotChanged)
            dispatcher.Dispatch(new CatalogLoadAction.Load());

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task OnSetPrice(CatalogPricingAction.SetPrice action, IDispatcher dispatcher)
    {
        if (action.PriceTypeId != state.Value.PricingOptions.PriceTypeId)
            dispatcher.Dispatch(new CatalogLoadAction.Load());

        return Task.CompletedTask;
    }
}
