namespace Media.Contracts.Folders;

/// <summary>
/// Lets another module lay out library files in a folder tree of its own (Catalog: product photos by category).
/// Folders are virtual, so moving a file never changes its URL.
/// </summary>
public interface IMediaFolderArranger
{
    /// <summary>
    /// Builds <paramref name="tree"/> under the top-level folder of the CDN directory <paramref name="storageDirectory"/>
    /// (for example <c>products</c> → «Товари») and moves every placed file into the folder of its node. Only files
    /// inside that top-level folder or its subfolders move; a file someone put elsewhere stays. Folders are made only
    /// for nodes that receive files (and their parents). <paramref name="dryRun"/> counts and saves nothing.
    /// </summary>
    Task<MediaFolderArrangeResult> ArrangeAsync(string storageDirectory, IReadOnlyCollection<MediaFolderTreeNode> tree,
        IReadOnlyCollection<MediaFilePlacement> placements, bool dryRun, CancellationToken cancellationToken);
}

/// <summary>One node of the caller's tree; <paramref name="ParentKey"/> <c>null</c> sits right under the top folder.</summary>
public sealed record MediaFolderTreeNode(int Key, string Name, int? ParentKey);

/// <summary>A library file and the node whose folder it belongs in.</summary>
public sealed record MediaFilePlacement(long FileId, int NodeKey);

/// <summary>What a run did (or would do): folders made, files moved, placed files left where someone put them.</summary>
public sealed record MediaFolderArrangeResult(int FoldersCreated, int FilesMoved, int FilesKeptOutside);
