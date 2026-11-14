using Media.Application.Abstractions.Images;
using Media.Application.Features.Files.Shared;

namespace Media.Application.Features.Files.Commands.UploadMediaFile;

/// <summary>Uploads a picture into <paramref name="FolderId"/> (<c>null</c> = library root).</summary>
public sealed record UploadMediaFileCommand(long? FolderId, MediaUpload File) : ICommand<Result<MediaFileItem>>;
