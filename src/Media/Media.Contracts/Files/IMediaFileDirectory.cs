namespace Media.Contracts.Files;

/// <summary>Read access to library files for other modules; they never read Media tables directly.</summary>
public interface IMediaFileDirectory
{
    /// <summary>Returns the files that exist among <paramref name="ids"/>; unknown IDs are left out.</summary>
    Task<IReadOnlyList<MediaFileReference>> GetByIdsAsync(IReadOnlyCollection<long> ids,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns the files whose CDN key lies directly in <paramref name="directory"/> (for example <c>products</c>
    /// gives <c>products/731.jpg</c> but not <c>products/old/731.jpg</c>).
    /// </summary>
    Task<IReadOnlyList<MediaFileReference>> ListInStorageDirectoryAsync(string directory,
        CancellationToken cancellationToken);
}
