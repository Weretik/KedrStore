using Media.Application.Features.Files.Common;

namespace Media.Application.Features.Files.Update;

/// <summary>Renames a file and puts it into <paramref name="FolderId"/> (<c>null</c> = library root).</summary>
public sealed record UpdateMediaFileCommand(long Id, string FileName, long? FolderId) : ICommand<Result<MediaFileItem>>;
