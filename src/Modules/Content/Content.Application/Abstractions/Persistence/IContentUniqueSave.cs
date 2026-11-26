namespace Content.Application.Abstractions.Persistence;

/// <summary>
/// Saves the pending changes of the content store and reports a broken unique index as <c>false</c> instead of an
/// exception, so a racing duplicate answers 409, not 500 (database rules 4).
/// </summary>
public interface IContentUniqueSave
{
    Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken);
}
