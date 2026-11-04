namespace Content.Application.Contracts.Gallery;

/// <summary>Reads media-library pictures by file ID; IDs that no longer exist are absent from the answer.</summary>
public interface IGalleryImageReader
{
    Task<IReadOnlyDictionary<long, GalleryImage>> GetImagesAsync(
        IReadOnlyCollection<long> mediaFileIds, CancellationToken cancellationToken);
}

public sealed record GalleryImage(string Url, string FileName, int? Width, int? Height);
