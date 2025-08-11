namespace Presentation.Shared.Common.Abstractions;

public interface IState
{
    event Action? OnChange;
    string StateName => GetType().Name;
    void Reset();
    object Snapshot();
}
