using Presentation.Shared.Extensions;

namespace Presentation.Shared.States.Catalog;

public static class CatalogPricingReducer
{
    [ReducerMethod]
    public static CatalogState OnSetPricingOptions(CatalogState state, CatalogPricingAction.SetPricingOptions action)
        => (state with { PricingOptions = action.Pricing }).ResetPage();

    [ReducerMethod]
    public static CatalogState OnSetPriceRange(CatalogState state, CatalogPricingAction.SetPriceRange action)
    {
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
    public static CatalogState OnSetPrice(CatalogState state, CatalogPricingAction.SetPriceId action)
    {
        var updateState =  state with
        {
            PricingOptions =  state.PricingOptions with { PriceType = action.PriceType }
        };
        return state;
    }
}
