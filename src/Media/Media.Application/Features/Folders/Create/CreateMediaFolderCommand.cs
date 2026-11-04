using Media.Application.Features.Folders.Common;

namespace Media.Application.Features.Folders.Create;

/// <summary>Creates a folder in <paramref name="ParentId"/>, or in the library root when it is <c>null</c>.</summary>
public sealed record CreateMediaFolderCommand(string Name, long? ParentId) : ICommand<Result<MediaFolderItem>>;
