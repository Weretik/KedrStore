using Media.Application.Contracts.Images;
using Media.Application.Features.Files.Common;

namespace Media.Application.Features.Files.Upload;

/// <summary>Uploads a picture into <paramref name="FolderId"/> (<c>null</c> = library root).</summary>
public sealed record UploadMediaFileCommand(long? FolderId, MediaUpload File) : ICommand<Result<MediaFileItem>>;
