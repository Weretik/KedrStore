namespace Content.Application.Contracts.MediaLibrary;

/// <summary>Puts a picture into the media library: on the CDN under a directory and into a root library folder.</summary>
public interface IMediaImageWriter
{
    /// <summary>503 without CDN credentials; 400 for bytes that are not a picture.</summary>
    Task<Result<StoredMediaImage>> UploadAsync(MediaImageUpload upload, CancellationToken cancellationToken);
}

public sealed record MediaImageUpload(string StorageDirectory, string FolderName, string FileName, byte[] Data);

public sealed record StoredMediaImage(long MediaFileId, string Url);
