namespace Catalog.Application.Contracts.Media;

/// <summary>A media library file as Catalog sees it.</summary>
public sealed record ProductMediaLibraryFile(long Id, string StorageKey, string FileName, string Url);

/// <summary>A category as a folder node; <paramref name="ParentId"/> <c>null</c> is a top category.</summary>
public sealed record ProductMediaFolderNode(int CategoryId, string Name, int? ParentId);

/// <summary>A library file and the category whose folder it belongs in.</summary>
public sealed record ProductMediaFolderPlacement(long FileId, int CategoryId);

/// <summary>Folders made, files moved, and placed files left where someone put them.</summary>
public sealed record ProductMediaFolderResult(int FoldersCreated, int FilesMoved, int FilesKeptOutside);

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

    /// <summary>
    /// Lays out the files of the CDN directory's library folder (<c>products</c> → «Товари») by the category tree.
    /// Files outside that folder stay where they are; <paramref name="dryRun"/> saves nothing.
    /// </summary>
    Task<ProductMediaFolderResult> ArrangeFoldersAsync(string directory,
        IReadOnlyCollection<ProductMediaFolderNode> categories,
        IReadOnlyCollection<ProductMediaFolderPlacement> placements, bool dryRun, CancellationToken cancellationToken);
}
