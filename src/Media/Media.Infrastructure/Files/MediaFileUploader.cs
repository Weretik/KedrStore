using Media.Application.Contracts.Images;
using Media.Application.Contracts.Storage;
using Media.Contracts.Files;

namespace Media.Infrastructure.Files;

/// <summary>
/// Serves <see cref="IMediaFileUploader"/>: CDN first, then the row; a failed save removes the new CDN file,
/// as the library's own upload does.
/// </summary>
internal sealed class MediaFileUploader(
    MediaDbContext db,
    IImageInspector inspector,
    IMediaStorage storage,
    IMediaUrlBuilder urls,
    TimeProvider clock)
    : IMediaFileUploader
{
    public async Task<MediaFileUploadResult> UploadAsync(MediaFileUploadRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!storage.IsConfigured)
            return new MediaFileUploadResult(MediaFileUploadStatus.StorageUnavailable, null);

        using var buffer = new MemoryStream(request.Content, writable: false);
        var image = inspector.Inspect(buffer);
        if (image is null)
            return new MediaFileUploadResult(MediaFileUploadStatus.NotAPicture, null);

        var now = clock.GetUtcNow();
        var folder = await RootFolderAsync(request.FolderName, now, cancellationToken);
        var directory = request.StorageDirectory.Trim().Trim('/');
        var key = $"{directory}/{Guid.NewGuid():N}{image.Extension}";
        buffer.Position = 0;
        await storage.UploadAsync(key, buffer, cancellationToken);

        try
        {
            var content = new MediaFileContent(key, image.ContentType, request.Content.Length, image.Width, image.Height);
            var file = MediaFile.Create(folder, request.FileName, content, now);
            db.MediaFiles.Add(file);
            await db.SaveChangesAsync(cancellationToken);
            return new MediaFileUploadResult(MediaFileUploadStatus.Uploaded, new MediaFileReference(
                file.Id.Value, file.StorageKey, file.FileName, urls.Build(file.StorageKey), file.Width, file.Height));
        }
        catch
        {
            await storage.DeleteAsync(key, CancellationToken.None);
            throw;
        }
    }

    /// <summary>The oldest root folder with this name (case-insensitive), created when there is none.</summary>
    private async Task<MediaFolderId> RootFolderAsync(string name, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var normalized = MediaFolder.NormalizeName(name);
        var lowered = normalized.ToLower();
        var existing = await db.MediaFolders.AsNoTracking()
            .Where(folder => folder.ParentId == null && folder.Name.ToLower() == lowered)
            .OrderBy(folder => folder.Id)
            .Select(folder => (MediaFolderId?)folder.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (existing is { } id)
            return id;

        var created = MediaFolder.Create(normalized, null, now);
        db.MediaFolders.Add(created);
        await db.SaveChangesAsync(cancellationToken);
        return created.Id;
    }
}
