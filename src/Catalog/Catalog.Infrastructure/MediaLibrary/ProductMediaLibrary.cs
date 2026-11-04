using Catalog.Application.Contracts.Media;
using Media.Contracts.Files;

namespace Catalog.Infrastructure.MediaLibrary;

/// <summary>Serves Catalog's media port through the Media contract.</summary>
internal sealed class ProductMediaLibrary(IMediaFileDirectory directory) : IProductMediaLibrary
{
    public async Task<IReadOnlyList<ProductMediaLibraryFile>> GetByIdsAsync(IReadOnlyCollection<long> ids,
        CancellationToken cancellationToken)
        => Map(await directory.GetByIdsAsync(ids, cancellationToken));

    public async Task<IReadOnlyList<ProductMediaLibraryFile>> ListInDirectoryAsync(string directoryName,
        CancellationToken cancellationToken)
        => Map(await directory.ListInStorageDirectoryAsync(directoryName, cancellationToken));

    private static ProductMediaLibraryFile[] Map(IEnumerable<MediaFileReference> files)
        => files.Select(file => new ProductMediaLibraryFile(file.Id, file.StorageKey, file.FileName, file.Url))
            .ToArray();
}
