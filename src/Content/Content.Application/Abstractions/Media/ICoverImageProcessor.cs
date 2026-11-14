namespace Content.Application.Abstractions.Media;

/// <summary>An uploaded cover file as the API received it.</summary>
public sealed record CoverUpload(string FileName, string ContentType, long Length, Func<Stream> OpenReadStream);

/// <summary>A cover ready to store: resized WebP bytes.</summary>
public sealed record ProcessedCover(byte[] Data, string ContentType);

/// <summary>Checks an uploaded picture and turns it into a web-sized WebP.</summary>
public interface ICoverImageProcessor
{
    Task<Result<ProcessedCover>> ProcessAsync(CoverUpload upload, CancellationToken cancellationToken = default);
}
