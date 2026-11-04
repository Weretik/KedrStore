using Content.Application.Contracts.Gallery;
using Content.Application.Features.Gallery.Common;

namespace Content.Application.Features.Gallery.Admin.Reorder;

/// <summary>
/// Saves the order of the whole gallery in one go. The list must hold exactly the current cards, so an order
/// built from a stale page (a card added or removed meanwhile) answers 409 instead of losing a card.
/// </summary>
public sealed class ReorderGalleryCommandHandler(
    IContentRepository<GalleryItem> repository,
    IReadContentDbContext db,
    IGalleryImageReader images,
    TimeProvider clock)
    : ICommandHandler<ReorderGalleryCommand, Result<IReadOnlyList<AdminGalleryItem>>>
{
    public async ValueTask<Result<IReadOnlyList<AdminGalleryItem>>> Handle(ReorderGalleryCommand command,
        CancellationToken cancellationToken)
    {
        var items = await repository.ListAsync(cancellationToken);
        if (items.Count != command.Ids.Count || items.Any(item => !command.Ids.Contains(item.Id)))
            return Result.Conflict("The gallery has changed; reload the page and try again.");

        var now = clock.GetUtcNow();
        var byId = items.ToDictionary(item => item.Id);
        for (var position = 0; position < command.Ids.Count; position++)
            byId[command.Ids[position]].MoveTo(position, now);
        await repository.UpdateRangeAsync(items, cancellationToken);
        return Result.Success(await GalleryAdminList.LoadAsync(db, images, cancellationToken));
    }
}
