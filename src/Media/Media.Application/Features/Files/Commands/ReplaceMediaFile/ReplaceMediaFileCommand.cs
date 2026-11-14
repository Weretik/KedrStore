using Media.Application.Abstractions.Images;
using Media.Application.Features.Files.Shared;

namespace Media.Application.Features.Files.Commands.ReplaceMediaFile;

/// <summary>Puts a new picture into an existing file: the ID stays, the URL changes (R-5).</summary>
public sealed record ReplaceMediaFileCommand(long Id, MediaUpload File) : ICommand<Result<MediaFileItem>>;
