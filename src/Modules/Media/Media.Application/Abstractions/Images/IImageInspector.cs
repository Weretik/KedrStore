namespace Media.Application.Abstractions.Images;

/// <summary>An uploaded file as the API received it.</summary>
public sealed record MediaUpload(string FileName, string ContentType, long Length, Func<Stream> OpenReadStream);

/// <summary>What the picture really is, read from its header: content type, file extension, size in pixels.</summary>
public sealed record ImageInfo(string ContentType, string Extension, int Width, int Height);

/// <summary>Reads the picture header without decoding the whole image.</summary>
public interface IImageInspector
{
    /// <summary><c>null</c> when the bytes are not a JPEG, PNG, or WebP picture.</summary>
    ImageInfo? Inspect(Stream content);
}
