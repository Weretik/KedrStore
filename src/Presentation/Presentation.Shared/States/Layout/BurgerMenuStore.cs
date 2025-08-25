namespace Presentation.Shared.States.Layout;

public sealed class BurgerMenuStore(IDispatcher dispatcher) : IBurgerMenuStore
{
    public void Open(bool value) => dispatcher.Dispatch(new BurgerMenuActions.SetOpen(value));
    public void Toggle() => dispatcher.Dispatch(new BurgerMenuActions.Toggle());
}
