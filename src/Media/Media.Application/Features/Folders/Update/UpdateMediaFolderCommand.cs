using Media.Application.Features.Folders.Common;

namespace Media.Application.Features.Folders.Update;

/// <summary>Renames a folder and puts it into <paramref name="ParentId"/> (<c>null</c> = library root).</summary>
public sealed record UpdateMediaFolderCommand(long Id, string Name, long? ParentId) : ICommand<Result<MediaFolderItem>>;
