namespace Presentation.Shared.States.Layout;

public static class BurgerMenuReducers
{
    [ReducerMethod]
    public static BurgerMenuState OnOpen(BurgerMenuState state, BurgerMenuActions.SetOpen action)
        => new (IsOpen: action.IsOpen);

    [ReducerMethod]
    public static BurgerMenuState OnToggle(BurgerMenuState state, BurgerMenuActions.Toggle _)
        => new (IsOpen: !state.IsOpen);
}
