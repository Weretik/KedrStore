namespace Presentation.Shared.Common;

public interface IState
{
    event Action? OnChange;
    string StateName => GetType().Name;
    void Reset();
    object Snapshot();
}
