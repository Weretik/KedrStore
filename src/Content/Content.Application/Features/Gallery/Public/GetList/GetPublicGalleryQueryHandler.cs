using Content.Application.Contracts.Gallery;
using Content.Application.Features.Gallery.Common;

namespace Content.Application.Features.Gallery.Public.GetList;

/// <summary>The site gallery in one language; a photo whose library file was deleted is left out.</summary>
public sealed class GetPublicGalleryQueryHandler(IReadContentDbContext db, IGalleryImageReader images)
    : IQueryHandler<GetPublicGalleryQuery, Result<IReadOnlyList<PublicGalleryItem>>>
{
    public async ValueTask<Result<IReadOnlyList<PublicGalleryItem>>> Handle(GetPublicGalleryQuery query,
        CancellationToken cancellationToken)
    {
        var uk = query.Language == "uk";
        var items = await db.GalleryItems.AsNoTracking()
            .OrderBy(item => item.Position).ThenBy(item => item.Id)
            .Select(item => new
            {
                item.Id, item.Kind, item.MediaFileId, item.YoutubeId,
                Title = uk ? item.TitleUk : item.TitleRu,
                Caption = uk ? item.CaptionUk : item.CaptionRu,
            })
            .ToListAsync(cancellationToken);
        var files = await images.GetImagesAsync(
            items.Where(item => item.MediaFileId is not null).Select(item => item.MediaFileId!.Value).ToList(),
            cancellationToken);

        var result = new List<PublicGalleryItem>(items.Count);
        foreach (var item in items)
        {
            if (item.Kind == GalleryItemKind.Video)
            {
                result.Add(new PublicGalleryItem(item.Id, item.Kind, item.Title, item.Caption,
                    YoutubeThumbnails.Url(item.YoutubeId!), YoutubeThumbnails.Width, YoutubeThumbnails.Height,
                    item.YoutubeId));
            }
            else if (item.MediaFileId is { } fileId && files.TryGetValue(fileId, out var file))
            {
                result.Add(new PublicGalleryItem(item.Id, item.Kind, item.Title, item.Caption,
                    file.Url, file.Width, file.Height, null));
            }
        }

        return Result.Success<IReadOnlyList<PublicGalleryItem>>(result);
    }
}
