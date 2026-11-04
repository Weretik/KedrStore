namespace Media.Api.Requests;

/// <summary>Name and place of a folder; <c>parentId</c> <c>null</c> = library root.</summary>
public sealed record SaveMediaFolderRequest(string Name, long? ParentId);

/// <summary>Display name and folder of a file; <c>folderId</c> <c>null</c> = library root.</summary>
public sealed record UpdateMediaFileRequest(string FileName, long? FolderId);

/// <summary>Files to move and the target folder; <c>folderId</c> <c>null</c> = library root.</summary>
public sealed record MoveMediaFilesRequest(IReadOnlyList<long> FileIds, long? FolderId);

/// <summary>How many files were moved.</summary>
public sealed record MoveMediaFilesResponse(int Moved);
