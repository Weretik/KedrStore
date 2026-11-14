using Content.Application.Abstractions.Media;
using Content.Application.Features.Gallery.Shared;

namespace Content.Application.Features.Gallery.Commands.AddGalleryVideo;

/// <summary>Appends a video; the same video twice answers 409.</summary>
public sealed class AddGalleryVideoHandler(
    IContentRepository<GalleryItem> repository,
    IReadContentDbContext db,
    IMediaImageReader images,
    TimeProvider clock)
    : ICommandHandler<AddGalleryVideoCommand, Result<IReadOnlyList<AdminGalleryItem>>>
{
    public async ValueTask<Result<IReadOnlyList<AdminGalleryItem>>> Handle(AddGalleryVideoCommand command,
        CancellationToken cancellationToken)
    {
        var youtubeId = YoutubeVideoId.Parse(command.Url);
        if (await db.GalleryItems.AnyAsync(item => item.YoutubeId == youtubeId, cancellationToken))
            return Result.Conflict("This video is already in the gallery.");

        var position = await GalleryAdminList.NextPositionAsync(db, cancellationToken);
        await repository.AddAsync(GalleryItem.CreateVideo(youtubeId, position, GalleryTexts.ToDomain(command.Texts),
            clock.GetUtcNow()), cancellationToken);
        return Result.Success(await GalleryAdminList.LoadAsync(db, images, cancellationToken));
    }
}
