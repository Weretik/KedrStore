namespace Media.Contracts.Files;

/// <summary>
/// Lets another module put a picture into the library (Content: article covers). The file lands on the CDN under
/// <see cref="MediaFileUploadRequest.StorageDirectory"/> and in the root library folder
/// <see cref="MediaFileUploadRequest.FolderName"/>, which is created when missing.
/// </summary>
public interface IMediaFileUploader
{
    Task<MediaFileUploadResult> UploadAsync(MediaFileUploadRequest request, CancellationToken cancellationToken);
}

public sealed record MediaFileUploadRequest(string StorageDirectory, string FolderName, string FileName, byte[] Content);

public enum MediaFileUploadStatus
{
    Uploaded,

    /// <summary>The CDN credentials are not configured (local runs, tests).</summary>
    StorageUnavailable,

    /// <summary>The bytes are not a JPEG, PNG, or WebP picture.</summary>
    NotAPicture,

    /// <summary>The file or folder name breaks a library rule; <see cref="MediaFileUploadResult.Message"/> says which.</summary>
    Invalid,
}

/// <summary>
/// <see cref="File"/> is set only when <see cref="Status"/> is <see cref="MediaFileUploadStatus.Uploaded"/>;
/// <see cref="Message"/> is the user-facing reason of an <see cref="MediaFileUploadStatus.Invalid"/> upload.
/// </summary>
public sealed record MediaFileUploadResult(MediaFileUploadStatus Status, MediaFileReference? File, string? Message = null);
