using Content.Application.Contracts.MediaLibrary;
using Content.Application.Features.Gallery.Common;

namespace Content.Application.Features.Gallery.Admin.Delete;

public sealed class DeleteGalleryItemCommandHandler(
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
