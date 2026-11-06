namespace Catalog.Application.Contracts.Media;

/// <summary>A swatch picture stored in the media library.</summary>
public sealed record StoredSwatchImage(long MediaFileId, string Url);

/// <summary>
/// Puts a swatch picture of an attribute value (a color or decor photo) into the media library: on the CDN under
/// <c>swatches/</c> and in the library folder «Кольори». Infrastructure serves it through the Media contract.
/// </summary>
public interface IAttributeSwatchStorage
{
    /// <summary>503 without CDN credentials; 400 for bytes that are not a JPEG, PNG, or WebP picture.</summary>
    Task<Result<StoredSwatchImage>> UploadAsync(string fileName, byte[] data, CancellationToken cancellationToken);
}
