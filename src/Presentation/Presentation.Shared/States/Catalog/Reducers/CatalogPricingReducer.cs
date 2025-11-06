using Presentation.Shared.Extensions;

namespace Presentation.Shared.States.Catalog;

public static class CatalogPricingReducer
{
    [ReducerMethod]
    public static CatalogState OnSetPricingOptions(CatalogState state, CatalogPricingAction.SetPricingOptions action)
        => state with { PricingOptions = action.Pricing };

    [ReducerMethod]
    public static CatalogState OnSetPriceRange(CatalogState state, CatalogPricingAction.SetPriceRange action)
    {
        var min = state.PricingOptions.MinPrice;
        var max = state.PricingOptions.MaxPrice;
        var hasNotChanged  = action.MinPrice == min && action.MaxPrice == max;

        if (hasNotChanged) return state;

        var updateState =  state with
        {
            PricingOptions = state.PricingOptions with
            {
                MinPrice = action.MinPrice,
                MaxPrice = action.MaxPrice
            }
        };
        return updateState.ResetPage();
    }

    [ReducerMethod]
    public static CatalogState OnSetPrice(CatalogState state, CatalogPricingAction.SetPriceType action)
    {
        if (action.PriceType == state.PricingOptions.PriceType) return state;

        var updateState =  state with
        {
            PricingOptions =  state.PricingOptions with { PriceType = action.PriceType }
        };
        return updateState;
    }
}
