using Media.Application.Features.Folders.Shared;

namespace Media.Application.Features.Folders.Queries.GetMediaFolderTree;

/// <summary>All library folders by name with their file counts.</summary>
public sealed record GetMediaFolderTreeQuery : IQuery<Result<MediaFolderTree>>;
