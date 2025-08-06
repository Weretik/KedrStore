namespace Presentation.Shared.States;

public class BurgerMenuState : IState
{
    private bool _isOpen;
    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen != value)
            {
                _isOpen = value;
                NotifyChanged();
            }
        }

    }

    public event Action? OnChange;
    private void NotifyChanged() => OnChange?.Invoke();

    public void Open() => IsOpen = true;

    public void Close() => IsOpen = false;

    public void Toggle() => IsOpen = !IsOpen;


    public void Reset() => IsOpen = false;
    public object Snapshot() => new { IsOpen };


}
