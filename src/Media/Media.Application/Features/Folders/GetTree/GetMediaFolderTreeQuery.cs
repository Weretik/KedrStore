using Media.Application.Features.Folders.Common;

namespace Media.Application.Features.Folders.GetTree;

/// <summary>All library folders by name with their file counts.</summary>
public sealed record GetMediaFolderTreeQuery : IQuery<Result<MediaFolderTree>>;
