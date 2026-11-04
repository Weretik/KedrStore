using Catalog.Application.Contracts.Media;
using Catalog.Contracts.Products.Media;

namespace Catalog.Application.Features.ProductMedia.Common;

internal static class ProductMediaMapper
{
    /// <summary>
    /// Builds the answer from the links; file names come from the library. A link to a file the library no longer
    /// has keeps its URL and an empty name until the next sync run drops it.
    /// </summary>
    public static ProductMediaDto ToDto(Product product, IReadOnlyCollection<ProductMediaLibraryFile> files)
    {
        var names = files.ToDictionary(file => file.Id, file => file.FileName);
        ProductMediaItemDto ToItem(ProductMediaLink link)
            => new(link.MediaFileId, link.Url, names.GetValueOrDefault(link.MediaFileId) ?? string.Empty);

        return new ProductMediaDto(
            product.Id.Value,
            product.Photos.Select(ToItem).ToArray(),
            product.Scheme is { } scheme ? ToItem(scheme) : null);
    }
}
