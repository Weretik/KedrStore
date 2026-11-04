using Catalog.Application.Contracts.Media;
using Media.Contracts.Files;
using Media.Contracts.Folders;

namespace Catalog.Infrastructure.MediaLibrary;

/// <summary>Serves Catalog's media port through the Media contract.</summary>
internal sealed class ProductMediaLibrary(IMediaFileDirectory directory, IMediaFolderArranger arranger)
    : IProductMediaLibrary
{
    public async Task<IReadOnlyList<ProductMediaLibraryFile>> GetByIdsAsync(IReadOnlyCollection<long> ids,
        CancellationToken cancellationToken)
        => Map(await directory.GetByIdsAsync(ids, cancellationToken));

    public async Task<IReadOnlyList<ProductMediaLibraryFile>> ListInDirectoryAsync(string directoryName,
        CancellationToken cancellationToken)
        => Map(await directory.ListInStorageDirectoryAsync(directoryName, cancellationToken));

    public async Task<ProductMediaFolderResult> ArrangeFoldersAsync(string directoryName,
        IReadOnlyCollection<ProductMediaFolderNode> categories,
        IReadOnlyCollection<ProductMediaFolderPlacement> placements, bool dryRun, CancellationToken cancellationToken)
    {
        var result = await arranger.ArrangeAsync(directoryName,
            categories.Select(node => new MediaFolderTreeNode(node.CategoryId, node.Name, node.ParentId)).ToArray(),
            placements.Select(placement => new MediaFilePlacement(placement.FileId, placement.CategoryId)).ToArray(),
            dryRun, cancellationToken);
        return new ProductMediaFolderResult(result.FoldersCreated, result.FilesMoved, result.FilesKeptOutside);
    }

    private static ProductMediaLibraryFile[] Map(IEnumerable<MediaFileReference> files)
        => files.Select(file => new ProductMediaLibraryFile(file.Id, file.StorageKey, file.FileName, file.Url))
            .ToArray();
}
