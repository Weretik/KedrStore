namespace Presentation.Shared.States.Layout;

using static BurgerMenuActions;

[SuppressMessage("ReSharper", "WithExpressionModifiesAllMembers")]
public static class BurgerMenuReducers
{
    [ReducerMethod]
    public static BurgerMenuState OnOpen(BurgerMenuState state, SetOpen action)
        => state with { IsOpen = action.IsOpen };

    [ReducerMethod]
    public static BurgerMenuState OnToggle(BurgerMenuState state, Toggle _)
        => state with { IsOpen =  !state.IsOpen};

}
