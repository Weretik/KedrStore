using Catalog.Application.Abstractions.Media;

namespace Catalog.Application.Features.Categories.Shared;

/// <summary>Looks up the link-preview picture of a category in the media library (spec catalog/010 R-03).</summary>
internal static class CategoryShareImage
{
    /// <summary>The CDN address, or <c>null</c> without a picture or when the file left the library.</summary>
    public static async Task<string?> UrlAsync(IProductMediaLibrary media, MediaFileId? id,
        CancellationToken cancellationToken)
    {
        if (id is not { } fileId)
            return null;
        var files = await media.GetByIdsAsync([fileId.Value], cancellationToken);
        return files.FirstOrDefault(file => file.Id == fileId.Value)?.Url;
    }
}
