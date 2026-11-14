using Catalog.Application.Features.ProductMedia.Shared;
using Media.Contracts.Files;

namespace Catalog.Infrastructure.Adapters.Media;

/// <summary>Receives library replace and delete events and updates the products that link the file (R-35).</summary>
internal sealed class CatalogMediaFileChangeListener(ProductMediaFileChanges changes) : IMediaFileChangeListener
{
    public Task OnFileReplacedAsync(long fileId, string url, CancellationToken cancellationToken)
        => changes.ApplyReplacedAsync(fileId, url, cancellationToken);

    public Task OnFileDeletedAsync(long fileId, CancellationToken cancellationToken)
        => changes.ApplyDeletedAsync(fileId, cancellationToken);
}
