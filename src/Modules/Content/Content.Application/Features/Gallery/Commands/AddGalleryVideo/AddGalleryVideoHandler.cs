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
        var parsed = YoutubeVideoId.Parse(command.Url);
        if (parsed.IsFailure)
            return parsed.Error!.ToResult<IReadOnlyList<AdminGalleryItem>>();
        var youtubeId = parsed.Value;
        if (await db.GalleryItems.AnyAsync(item => item.YoutubeId == youtubeId, cancellationToken))
            return Result.Conflict("Це відео вже є в галереї.");

        var texts = GalleryTexts.ToDomain(command.Texts);
        if (texts.IsFailure)
            return texts.Error!.ToResult<IReadOnlyList<AdminGalleryItem>>();
        var position = await GalleryAdminList.NextPositionAsync(db, cancellationToken);
        var video = GalleryItem.CreateVideo(youtubeId, position, texts.Value, clock.GetUtcNow());
        if (video.IsFailure)
            return video.Error!.ToResult<IReadOnlyList<AdminGalleryItem>>();
        await repository.AddAsync(video.Value, cancellationToken);
        return Result.Success(await GalleryAdminList.LoadAsync(db, images, cancellationToken));
    }
}
