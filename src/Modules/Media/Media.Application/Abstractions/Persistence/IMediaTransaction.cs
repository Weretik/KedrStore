namespace Media.Application.Abstractions.Persistence;

/// <summary>
/// One database transaction around a Media step whose later saves need the database id of a row saved earlier
/// (database rules 3.1, 3.3): either every save of the step stays, or none does.
/// </summary>
public interface IMediaTransaction
{
    /// <summary>Runs <paramref name="work"/> in a transaction and commits when it finishes; an exception rolls every save back.</summary>
    Task RunAsync(Func<CancellationToken, Task> work, CancellationToken cancellationToken);
}
