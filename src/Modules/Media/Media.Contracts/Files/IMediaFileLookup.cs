namespace Media.Contracts.Files;

/// <summary>
/// Read port for other modules that link to library files by ID (Content gallery, later Catalog).
/// Unknown IDs are simply absent from the answer, so a deleted file never breaks the caller.
/// </summary>
public interface IMediaFileLookup
{
    Task<IReadOnlyDictionary<long, MediaFileReference>> GetFilesAsync(
        IReadOnlyCollection<long> ids, CancellationToken cancellationToken);
}

