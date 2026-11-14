namespace Media.Application.Features.Folders.Shared;

/// <summary>A folder with the number of files directly in it.</summary>
public sealed record MediaFolderItem(long Id, long? ParentId, string Name, int FileCount);

/// <summary>All folders (flat, the client builds the tree) and file counts of the root and the whole library.</summary>
public sealed record MediaFolderTree(IReadOnlyList<MediaFolderItem> Folders, int RootFileCount, int TotalFileCount);
