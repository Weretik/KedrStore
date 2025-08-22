namespace Presentation.Shared.States.Layout;

public interface IBurgerMenuFacade
{
    bool IsOpen { get;}
    void Open(bool value);
    void Toggle();
}
