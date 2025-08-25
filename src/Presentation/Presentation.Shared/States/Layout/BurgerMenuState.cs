namespace Presentation.Shared.States.Layout;

[FeatureState]
public sealed record BurgerMenuState(bool IsOpen = false)
{
    private BurgerMenuState() : this(false) { }
}
