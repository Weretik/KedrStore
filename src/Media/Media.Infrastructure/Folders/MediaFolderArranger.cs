using Media.Application.Jobs.CdnSync;
using Media.Contracts.Folders;
using Microsoft.Extensions.Options;

namespace Media.Infrastructure.Folders;

/// <summary>
/// Serves <see cref="IMediaFolderArranger"/> on the library tables. A real run saves each new folder at once to get
/// its ID, then moves the files in one save; a dry run gives new folders negative IDs and saves nothing.
/// </summary>
internal sealed class MediaFolderArranger(MediaDbContext db, IOptions<MediaCdnSyncOptions> options, TimeProvider clock)
    : IMediaFolderArranger
{
    private const string UnnamedFolder = "Без назви";

    public async Task<MediaFolderArrangeResult> ArrangeAsync(string storageDirectory,
        IReadOnlyCollection<MediaFolderTreeNode> tree, IReadOnlyCollection<MediaFilePlacement> placements, bool dryRun,
        CancellationToken cancellationToken)
    {
        var directory = storageDirectory.Trim().Trim('/');
        var rootName = options.Value.FolderNames.TryGetValue(directory, out var mapped) ? mapped : directory;
        var run = new Run(db, clock.GetUtcNow(), dryRun,
            await db.MediaFolders.AsNoTracking().Select(folder => new FolderRow(folder.Id.Value, folder.ParentId == null ? null : folder.ParentId.Value.Value, folder.Name))
                .ToListAsync(cancellationToken));

        var root = await run.ChildAsync(null, rootName, cancellationToken);
        var managed = run.Subtree(root);
        var nodes = tree.GroupBy(node => node.Key).ToDictionary(group => group.Key, group => group.First());
        var fileIds = placements.Select(placement => placement.FileId).Where(id => id > 0).Distinct()
            .Select(MediaFileId.Create).ToArray();
        var files = await db.MediaFiles.Where(file => fileIds.Contains(file.Id))
            .ToDictionaryAsync(file => file.Id.Value, cancellationToken);

        int moved = 0, keptOutside = 0;
        var folderOfNode = new Dictionary<int, long>();
        foreach (var placement in placements.DistinctBy(placement => placement.FileId))
        {
            if (!files.TryGetValue(placement.FileId, out var file) || !nodes.ContainsKey(placement.NodeKey))
                continue;
            if (file.FolderId is not { } current || !managed.Contains(current.Value))
            {
                keptOutside++;
                continue;
            }

            var target = await FolderOfAsync(placement.NodeKey, nodes, folderOfNode, run, root, cancellationToken);
            if (target == current.Value)
                continue;
            moved++;
            if (!dryRun)
                file.MoveTo(MediaFolderId.Create(target), run.Now);
        }

        if (!dryRun && moved > 0)
            await db.SaveChangesAsync(cancellationToken);
        return new MediaFolderArrangeResult(run.Created, moved, keptOutside);
    }

    private static async Task<long> FolderOfAsync(int key, Dictionary<int, MediaFolderTreeNode> nodes,
        Dictionary<int, long> cache, Run run, long root, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(key, out var known))
            return known;

        // Walk up to the top of the caller's tree; a missing parent or a cycle stops at the top folder.
        var path = new List<MediaFolderTreeNode>();
        var seen = new HashSet<int>();
        for (int? cursor = key; cursor is { } current && seen.Add(current) && nodes.TryGetValue(current, out var node);
             cursor = node.ParentKey)
            path.Add(node);
        path.Reverse();

        var parent = root;
        foreach (var node in path)
        {
            if (!cache.TryGetValue(node.Key, out var folder))
            {
                folder = await run.ChildAsync(parent, node.Name, cancellationToken);
                cache[node.Key] = folder;
            }
            parent = folder;
        }
        return parent;
    }

    private sealed record FolderRow(long Id, long? ParentId, string Name);

    /// <summary>Folders of one run, found by parent and name (case-insensitive), the oldest first.</summary>
    private sealed class Run(MediaDbContext db, DateTimeOffset now, bool dryRun, List<FolderRow> folders)
    {
        private readonly Dictionary<(long, string), long> _byName = folders
            .GroupBy(folder => (folder.ParentId ?? 0, folder.Name.ToLowerInvariant()))
            .ToDictionary(group => group.Key, group => group.Min(folder => folder.Id));
        private long _nextDryId;

        public DateTimeOffset Now => now;
        public int Created { get; private set; }

        public async Task<long> ChildAsync(long? parent, string name, CancellationToken cancellationToken)
        {
            var normalized = Normalize(name);
            var key = (parent ?? 0, normalized.ToLowerInvariant());
            if (_byName.TryGetValue(key, out var existing))
                return existing;

            long id;
            if (dryRun)
                id = --_nextDryId;
            else
            {
                // Normalize keeps the name within the rules, so the folder is always valid here.
                var folder = MediaFolder.Create(normalized,
                    parent is { } parentId ? MediaFolderId.Create(parentId) : null, now).Value;
                db.MediaFolders.Add(folder);
                await db.SaveChangesAsync(cancellationToken);
                id = folder.Id.Value;
            }

            Created++;
            _byName[key] = id;
            folders.Add(new FolderRow(id, parent, normalized));
            return id;
        }

        /// <summary>The folder and all its subfolders.</summary>
        public HashSet<long> Subtree(long root)
        {
            var children = folders.Where(folder => folder.ParentId is not null)
                .ToLookup(folder => folder.ParentId!.Value, folder => folder.Id);
            var result = new HashSet<long> { root };
            var queue = new Queue<long>([root]);
            while (queue.TryDequeue(out var current))
                foreach (var child in children[current])
                    if (result.Add(child))
                        queue.Enqueue(child);
            return result;
        }

        private static string Normalize(string name)
        {
            var trimmed = string.IsNullOrWhiteSpace(name) ? UnnamedFolder : name.Trim();
            return trimmed.Length > MediaFolder.NameMaxLength ? trimmed[..MediaFolder.NameMaxLength] : trimmed;
        }
    }
}
