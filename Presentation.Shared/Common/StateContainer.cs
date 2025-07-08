namespace Presentation.Shared.Common;

public sealed class StateContainer
{
    private readonly Dictionary<Type, IState> _states;

    public StateContainer(IEnumerable<IState> states)
    {
        _states = states.ToDictionary(s => s.GetType(), s => s);
    }

    public T Get<T>() where T : class, IState
    {
        if (_states.TryGetValue(typeof(T), out var state))
            return state as T;

        Throw.Application(AppErrors.State.NotRegistered<T>());
        return null!;
    }

    public void ResetAll() =>
        _states.Values.ToList().ForEach(s => s.Reset());

    public Dictionary<string, object> SnapshotAll() =>
        _states.Values.ToDictionary(s => s.StateName, s => s.Snapshot());
}
