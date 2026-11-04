namespace Catalog.Application.Contracts.Media;

/// <summary>A media library file as Catalog sees it.</summary>
public sealed record ProductMediaLibraryFile(long Id, string StorageKey, string FileName, string Url);

/// <summary>
/// Catalog's port to the media library. Infrastructure serves it through the Media contract; Catalog never reads
/// Media tables.
/// </summary>
public interface IProductMediaLibrary
{
    Task<IReadOnlyList<ProductMediaLibraryFile>> GetByIdsAsync(IReadOnlyCollection<long> ids,
        CancellationToken cancellationToken);

    /// <summary>Files whose CDN key lies directly in <paramref name="directory"/>, without subdirectories.</summary>
    Task<IReadOnlyList<ProductMediaLibraryFile>> ListInDirectoryAsync(string directory,
        CancellationToken cancellationToken);
}
