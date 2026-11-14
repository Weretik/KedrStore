namespace Scheduling.Application.Abstractions;

/// <summary>
/// Work the tick does itself, in the API (the outbox). The host binds it to the task key; the summary or the
/// exception's message is what the admin sees.
/// </summary>
public interface ITickWork
{
    string TaskKey { get; }

    Task<string> RunAsync(CancellationToken cancellationToken);
}
