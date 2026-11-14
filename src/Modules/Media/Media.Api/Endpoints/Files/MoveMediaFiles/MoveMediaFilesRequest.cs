namespace Media.Api.Endpoints.Files.MoveMediaFiles;

/// <summary>Files to move and the target folder; <c>folderId</c> <c>null</c> = library root.</summary>
public sealed record MoveMediaFilesRequest(IReadOnlyList<long> FileIds, long? FolderId);
