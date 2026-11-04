namespace Content.Application.Contracts.MediaLibrary;

/// <summary>Reads media-library pictures by file ID; IDs that no longer exist are absent from the answer.</summary>
public interface IMediaImageReader
{
    Task<IReadOnlyDictionary<long, MediaImage>> GetImagesAsync(
        IReadOnlyCollection<long> mediaFileIds, CancellationToken cancellationToken);
}

public sealed record MediaImage(string Url, string FileName, int? Width, int? Height);
