using Media.Application.Features.Files.Shared;

namespace Media.Application.Features.Files.Queries.GetMediaFiles;

/// <summary>
/// A page of files by name. <paramref name="FolderId"/> limits it to one folder, <paramref name="InRoot"/> to the
/// library root; with neither, the whole library. <paramref name="IncludeSubfolders"/> adds the files of all
/// subfolders of <paramref name="FolderId"/>. <paramref name="Search"/> matches the display name.
/// </summary>
public sealed record GetMediaFilesQuery(long? FolderId, bool InRoot, string? Search, int Page = 1, int PageSize = 60,
    bool IncludeSubfolders = false)
    : IQuery<Result<MediaFilePage>>;
