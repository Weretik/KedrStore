namespace Presentation.Shared.States.Layout;

public sealed class BurgerMenuFacade(
    IState<BurgerMenuState> state, IDispatcher dispatcher)
    : IBurgerMenuFacade
{
    public bool IsOpen => state.Value.IsOpen;
    public void Open(bool value) => dispatcher.Dispatch(new BurgerMenuActions.SetOpen(value));
    public void Toggle() => dispatcher.Dispatch(new BurgerMenuActions.Toggle());
}
