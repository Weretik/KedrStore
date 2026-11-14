using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.ProductMedia.Shared;

namespace Catalog.Application.Features.ProductMedia.Shared;

/// <summary>
/// Applies a replace or delete in the media library to every product that links the file (R-35). Called by the
/// Media change listener after the library saved its change; a failure is repaired by the next sync run (R-34).
/// </summary>
public sealed class ProductMediaFileChanges(
    ICatalogRepository<Product> products,
    IProductListProjectionRebuilder projections,
    TimeProvider clock)
{
    public Task ApplyReplacedAsync(long fileId, string url, CancellationToken cancellationToken)
        => ApplyAsync(fileId, (product, now) => product.UpdateMediaUrl(fileId, url, now), cancellationToken);

    public Task ApplyDeletedAsync(long fileId, CancellationToken cancellationToken)
        => ApplyAsync(fileId, (product, now) => product.RemoveMediaFile(fileId, now), cancellationToken);

    private async Task ApplyAsync(long fileId, Func<Product, DateTimeOffset, bool> apply,
        CancellationToken cancellationToken)
    {
        var linked = await products.ListAsync(new ProductsLinkedToMediaFileSpec(fileId), cancellationToken);
        var now = clock.GetUtcNow();
        var changed = linked.Count(product => apply(product, now));
        if (changed == 0)
            return;

        await products.SaveChangesAsync(cancellationToken);
        await projections.RefreshPhotosAsync(cancellationToken);
    }
}
