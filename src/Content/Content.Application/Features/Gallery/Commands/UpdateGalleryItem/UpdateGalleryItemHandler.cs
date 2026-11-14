using Content.Application.Abstractions.Media;
using Content.Application.Features.Gallery.Shared;

namespace Content.Application.Features.Gallery.Commands.UpdateGalleryItem;

/// <summary>Replaces the title and caption of a card in both languages.</summary>
public sealed class UpdateGalleryItemHandler(
    IContentRepository<GalleryItem> repository,
    IReadContentDbContext db,
    IMediaImageReader images,
    TimeProvider clock)
    : ICommandHandler<UpdateGalleryItemCommand, Result<IReadOnlyList<AdminGalleryItem>>>
{
    public async ValueTask<Result<IReadOnlyList<AdminGalleryItem>>> Handle(UpdateGalleryItemCommand command,
        CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (item is null)
            return Result.NotFound();

        var texts = GalleryTexts.ToDomain(command.Texts);
        if (texts.IsFailure)
            return texts.Error!.ToResult<IReadOnlyList<AdminGalleryItem>>();
        item.UpdateTexts(texts.Value, clock.GetUtcNow());
        await repository.UpdateAsync(item, cancellationToken);
        return Result.Success(await GalleryAdminList.LoadAsync(db, images, cancellationToken));
    }
}
