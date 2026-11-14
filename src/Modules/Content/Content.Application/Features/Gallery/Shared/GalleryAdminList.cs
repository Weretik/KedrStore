using Content.Application.Abstractions.Media;

namespace Content.Application.Features.Gallery.Shared;

/// <summary>The whole gallery in site order with previews; every admin change answers with it.</summary>
public static class GalleryAdminList
{
    public static async Task<IReadOnlyList<AdminGalleryItem>> LoadAsync(IReadContentDbContext db,
        IMediaImageReader images, CancellationToken cancellationToken)
    {
        var items = await db.GalleryItems.AsNoTracking()
            .OrderBy(item => item.Position).ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);
        var files = await images.GetImagesAsync(
            items.Where(item => item.MediaFileId is not null).Select(item => item.MediaFileId!.Value).ToList(),
            cancellationToken);

        return items.Select(item =>
        {
            MediaImage? file = null;
            if (item.MediaFileId is { } fileId)
                files.TryGetValue(fileId, out file);
            var preview = item.Kind == GalleryItemKind.Video ? YoutubeThumbnails.Url(item.YoutubeId!) : file?.Url;
            return new AdminGalleryItem(item.Id, item.Kind, item.MediaFileId, item.YoutubeId, item.Position,
                item.TitleUk, item.TitleRu, item.CaptionUk, item.CaptionRu, preview, file?.FileName,
                item.Kind == GalleryItemKind.Photo && file is null, item.UpdatedAt);
        }).ToList();
    }

    /// <summary>The position after the last card, so new cards go to the end.</summary>
    public static async Task<int> NextPositionAsync(IReadContentDbContext db, CancellationToken cancellationToken)
        => await db.GalleryItems.AnyAsync(cancellationToken)
            ? await db.GalleryItems.MaxAsync(item => item.Position, cancellationToken) + 1
            : 0;
}
