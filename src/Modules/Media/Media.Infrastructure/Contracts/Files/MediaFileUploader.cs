using Media.Application.Abstractions.Images;
using Media.Application.Abstractions.Storage;
using Media.Contracts.Files;

namespace Media.Infrastructure.Contracts.Files;

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
        var fileName = MediaFile.NormalizeFileName(request.FileName);
        if (fileName.IsFailure)
            return Invalid(fileName.Error!);
        var folder = await RootFolderAsync(request.FolderName, now, cancellationToken);
        if (folder.IsFailure)
            return Invalid(folder.Error!);
        var directory = request.StorageDirectory.Trim().Trim('/');
        var key = $"{directory}/{Guid.NewGuid():N}{image.Extension}";
        buffer.Position = 0;
        await storage.UploadAsync(key, buffer, cancellationToken);

        try
        {
            var content = new MediaFileContent(key, image.ContentType, request.Content.Length, image.Width, image.Height);
            var created = MediaFile.Create(folder.Value, fileName.Value, content, now);
            if (created.IsFailure)
            {
                await storage.DeleteAsync(key, CancellationToken.None);
                return Invalid(created.Error!);
            }

            var file = created.Value;
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

    private static MediaFileUploadResult Invalid(DomainError error)
        => new(MediaFileUploadStatus.Invalid, null, error.Message);

    /// <summary>The oldest root folder with this name (case-insensitive), created when there is none.</summary>
    private async Task<DomainResult<MediaFolderId>> RootFolderAsync(string name, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var normalizedName = MediaFolder.NormalizeName(name);
        if (normalizedName.IsFailure)
            return normalizedName.Error!;
        var normalized = normalizedName.Value;
        var lowered = normalized.ToLower();
        var existing = await db.MediaFolders.AsNoTracking()
            .Where(folder => folder.ParentId == null && folder.Name.ToLower() == lowered)
            .OrderBy(folder => folder.Id)
            .Select(folder => (MediaFolderId?)folder.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (existing is { } id)
            return id;

        var created = MediaFolder.Create(normalized, null, now);
        if (created.IsFailure)
            return created.Error!;
        db.MediaFolders.Add(created.Value);
        await db.SaveChangesAsync(cancellationToken);
        return created.Value.Id;
    }
}
