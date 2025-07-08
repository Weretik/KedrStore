namespace Presentation.Shared.States;

public class BurgerMenuState : IState
{
    public bool IsOpen { get; private set; }

    public event Action? OnChange;
    private void NotifyChanged() => OnChange?.Invoke();

    public void Open()
    {
        IsOpen = true;
        NotifyChanged();
    }
    public void Close()
    {
        IsOpen = false;
        NotifyChanged();
    }
    public void Toggle()
    {
        IsOpen = !IsOpen;
        NotifyChanged();
    }

    public void Reset() => IsOpen = false;
    public object Snapshot() => new { IsOpen };


}
