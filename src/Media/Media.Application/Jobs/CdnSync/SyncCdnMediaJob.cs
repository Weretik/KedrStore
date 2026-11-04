using Media.Application.Contracts.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Media.Application.Jobs.CdnSync;

/// <summary>
/// Registers CDN pictures the library does not know yet (R-8): the directory path becomes the folder path,
/// known keys are skipped, and files missing on the CDN are only reported. Safe to run again.
/// </summary>
public sealed class SyncCdnMediaJob(
    IMediaStorage storage,
    IMediaRepository<MediaFolder> folders,
    IMediaRepository<MediaFile> files,
    IReadMediaDbContext db,
    IOptions<MediaCdnSyncOptions> options,
    TimeProvider clock,
    ILogger<SyncCdnMediaJob> logger)
{
    private readonly MediaCdnSyncOptions _options = options.Value;

    public async Task<MediaCdnSyncReport> RunAsync(bool dryRun, CancellationToken cancellationToken)
    {
        if (!storage.IsConfigured)
            throw new InvalidOperationException("Media storage is not configured: set MediaStorage:Ftp.");

        var listed = await storage.ListAsync(_options.ExcludedDirectories, cancellationToken);
        // The CDN keeps its own service folders such as `.thumbs/`; they are not pictures of the shop.
        var objects = listed.Where(item => !IsHidden(item.Key)).ToList();
        var hidden = listed.Count - objects.Count;
        var knownKeys = (await db.MediaFiles.AsNoTracking().Select(file => file.StorageKey).ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.Ordinal);
        var cdnKeys = objects.Select(item => item.Key).ToHashSet(StringComparer.Ordinal);

        var notPictures = 0;
        var orphans = 0;
        var known = 0;
        var toRegister = new List<StoredMediaObject>();
        foreach (var item in objects)
        {
            if (knownKeys.Contains(item.Key)) known++;
            else if (MediaContentTypes.FromKey(item.Key) is null) notPictures++;
            else if (IsOwned(item.Key)) orphans++;
            else toRegister.Add(item);
        }

        var missing = knownKeys.Count(key => !cdnKeys.Contains(key) && !IsExcluded(key) && !IsHidden(key));
        var byFolder = toRegister
            .GroupBy(item => FolderPathOf(item.Key) is { Length: > 0 } path ? string.Join(" / ", path) : "/", StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);

        if (!dryRun && toRegister.Count > 0)
            await RegisterAsync(toRegister, cancellationToken);

        var report = new MediaCdnSyncReport(dryRun, objects.Count, known, toRegister.Count, notPictures, orphans, missing, hidden, byFolder);
        logger.LogInformation(
            "Media CDN sync (dryRun={DryRun}): found {Found}, known {Known}, registered {Registered}, not pictures {NotPictures}, orphans {Orphans}, missing on CDN {Missing}, hidden skipped {Hidden}",
            report.DryRun, report.FoundOnCdn, report.AlreadyKnown, report.Registered, report.NotPictures, report.Orphans,
            report.MissingOnCdn, report.HiddenSkipped);
        foreach (var (folder, count) in byFolder.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            logger.LogInformation("Media CDN sync folder {Folder}: {Count} new files", folder, count);
        return report;
    }

    private async Task RegisterAsync(IReadOnlyList<StoredMediaObject> items, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var existing = await db.MediaFolders.AsNoTracking()
            .Select(folder => new { folder.Id, folder.ParentId, folder.Name })
            .ToListAsync(cancellationToken);
        var folderIds = existing.ToDictionary(folder => FolderKey(folder.ParentId, folder.Name), folder => folder.Id);

        foreach (var batch in items.Chunk(Math.Max(1, _options.BatchSize)))
        {
            var created = new List<MediaFile>(batch.Length);
            foreach (var item in batch)
            {
                var folderId = await EnsureFolderAsync(FolderPathOf(item.Key), folderIds, now, cancellationToken);
                var content = new MediaFileContent(item.Key, MediaContentTypes.FromKey(item.Key)!, item.SizeBytes, null, null);
                created.Add(MediaFile.Create(folderId, Path.GetFileName(item.Key), content, now));
            }
            await files.AddRangeAsync(created, cancellationToken);
        }
    }

    private async Task<MediaFolderId?> EnsureFolderAsync(IReadOnlyList<string> path,
        Dictionary<string, MediaFolderId> folderIds, DateTimeOffset now, CancellationToken cancellationToken)
    {
        MediaFolderId? parent = null;
        foreach (var name in path)
        {
            var key = FolderKey(parent, name);
            if (!folderIds.TryGetValue(key, out var id))
            {
                var folder = await folders.AddAsync(MediaFolder.Create(name, parent, now), cancellationToken);
                id = folder.Id;
                folderIds[key] = id;
            }
            parent = id;
        }
        return parent;
    }

    /// <summary>Folder names for the directories of a key; the first level is renamed by <see cref="MediaCdnSyncOptions.FolderNames"/>.</summary>
    private string[] FolderPathOf(string key)
    {
        var directories = key.Split('/', StringSplitOptions.RemoveEmptyEntries)[..^1];
        if (directories.Length > 0 && _options.FolderNames.TryGetValue(directories[0], out var name))
            directories[0] = name;
        return directories.Select(directory => directory.Length > MediaFolder.NameMaxLength
            ? directory[..MediaFolder.NameMaxLength]
            : directory).ToArray();
    }

    /// <summary>A file in a directory whose name starts with a dot (`.thumbs/…`) is a CDN service file.</summary>
    private static bool IsHidden(string key)
        => key.Split('/', StringSplitOptions.RemoveEmptyEntries)[..^1].Any(directory => directory.StartsWith('.'));

    private bool IsOwned(string key) => HasTopDirectory(key, _options.OwnedDirectories);

    private bool IsExcluded(string key) => HasTopDirectory(key, _options.ExcludedDirectories);

    private static bool HasTopDirectory(string key, IEnumerable<string> directories)
        => directories.Any(directory => key.StartsWith(directory.Trim('/') + "/", StringComparison.OrdinalIgnoreCase));

    private static string FolderKey(MediaFolderId? parent, string name)
        => $"{parent?.Value ?? 0}/{name.ToLowerInvariant()}";
}
