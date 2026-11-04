using Media.Application.Contracts.Images;
using Media.Application.Features.Files.Common;

namespace Media.Application.Features.Files.Replace;

/// <summary>Puts a new picture into an existing file: the ID stays, the URL changes (R-5).</summary>
public sealed record ReplaceMediaFileCommand(long Id, MediaUpload File) : ICommand<Result<MediaFileItem>>;
