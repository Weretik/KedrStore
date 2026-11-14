namespace Media.Api.Endpoints.Folders;

/// <summary>Name and place of a folder; <c>parentId</c> <c>null</c> = library root.</summary>
public sealed record SaveMediaFolderRequest(string Name, long? ParentId);
