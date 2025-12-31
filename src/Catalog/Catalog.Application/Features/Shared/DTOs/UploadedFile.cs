namespace Catalog.Application.Shared;

public sealed record UploadedFile(
    string FileName,
    string? ContentType,
    long Length,
    Func<Stream> OpenReadStream
);
