namespace Catalog.Application.Abstractions.Persistence;

/// <summary>
/// One database transaction around a Catalog command whose later steps need the database id of a row saved earlier
/// (database rules 3.1, 3.2): either every save of the command stays, or none does.
/// </summary>
public interface ICatalogTransaction
{
    /// <summary>
    /// Runs <paramref name="work"/> in a transaction and commits only when it returns a success; a failure or an
    /// exception rolls every save back. Inside an outer transaction it just runs.
    /// </summary>
    Task<Result<T>> RunAsync<T>(Func<CancellationToken, Task<Result<T>>> work, CancellationToken cancellationToken);
}
