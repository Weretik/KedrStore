using Content.Application.Abstractions.Media;
using Content.Application.Features.Gallery.Shared;

namespace Content.Application.Features.Gallery.Commands.DeleteGalleryItem;

public sealed class DeleteGalleryItemHandler(
    IContentRepository<GalleryItem> repository,
    IReadContentDbContext db,
    IMediaImageReader images)
    : ICommandHandler<DeleteGalleryItemCommand, Result<IReadOnlyList<AdminGalleryItem>>>
{
    public async ValueTask<Result<IReadOnlyList<AdminGalleryItem>>> Handle(DeleteGalleryItemCommand command,
        CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (item is null)
            return Result.NotFound();

        await repository.DeleteAsync(item, cancellationToken);
        return Result.Success(await GalleryAdminList.LoadAsync(db, images, cancellationToken));
    }
}
