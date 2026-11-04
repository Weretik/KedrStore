using Media.Application.Features.Files.Common;

namespace Media.Application.Features.Files.GetList;

/// <summary>
/// A page of files by name. <paramref name="FolderId"/> limits it to one folder, <paramref name="InRoot"/> to the
/// library root; with neither, the whole library. <paramref name="Search"/> matches the display name.
/// </summary>
public sealed record GetMediaFilesQuery(long? FolderId, bool InRoot, string? Search, int Page = 1, int PageSize = 60)
    : IQuery<Result<MediaFilePage>>;
