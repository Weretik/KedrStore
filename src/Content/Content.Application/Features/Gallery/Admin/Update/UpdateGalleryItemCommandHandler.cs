using Content.Application.Contracts.Gallery;
using Content.Application.Features.Gallery.Common;

namespace Content.Application.Features.Gallery.Admin.Update;

/// <summary>Replaces the title and caption of a card in both languages.</summary>
public sealed class UpdateGalleryItemCommandHandler(
    IContentRepository<GalleryItem> repository,
    IReadContentDbContext db,
    IGalleryImageReader images,
    TimeProvider clock)
    : ICommandHandler<UpdateGalleryItemCommand, Result<IReadOnlyList<AdminGalleryItem>>>
{
    public async ValueTask<Result<IReadOnlyList<AdminGalleryItem>>> Handle(UpdateGalleryItemCommand command,
        CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (item is null)
            return Result.NotFound();

        item.UpdateTexts(GalleryTexts.ToDomain(command.Texts), clock.GetUtcNow());
        await repository.UpdateAsync(item, cancellationToken);
        return Result.Success(await GalleryAdminList.LoadAsync(db, images, cancellationToken));
    }
}
