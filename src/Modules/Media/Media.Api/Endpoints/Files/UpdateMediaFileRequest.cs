namespace Media.Api.Endpoints.Files;

/// <summary>Display name and folder of a file; <c>folderId</c> <c>null</c> = library root.</summary>
public sealed record UpdateMediaFileRequest(string FileName, long? FolderId);
