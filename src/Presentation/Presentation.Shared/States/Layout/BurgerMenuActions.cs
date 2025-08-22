namespace Presentation.Shared.States.Layout;

public static class BurgerMenuActions
{
    public sealed record SetOpen(bool IsOpen);
    public sealed record Toggle;
}
