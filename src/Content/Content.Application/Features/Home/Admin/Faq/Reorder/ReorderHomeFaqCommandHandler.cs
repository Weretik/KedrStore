using Content.Application.Contracts.MediaLibrary;
using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Faq.Reorder;

/// <summary>Saves the whole order; a list built from a stale page answers 409 instead of losing a row.</summary>
public sealed class ReorderHomeFaqCommandHandler(
    IContentRepository<HomeFaqItem> repository,
    IReadContentDbContext db,
    IMediaImageReader images,
    TimeProvider clock)
    : ICommandHandler<ReorderHomeFaqCommand, Result<AdminHomeBlocks>>
{
    public async ValueTask<Result<AdminHomeBlocks>> Handle(ReorderHomeFaqCommand command, CancellationToken cancellationToken)
    {
        var items = await repository.ListAsync(cancellationToken);
        var now = clock.GetUtcNow();
        if (!HomeAdminBlocks.TryReorder(items, command.Ids, item => item.Id, (item, position) => item.MoveTo(position, now)))
            return Result.Conflict("The list has changed; reload the page and try again.");

        await repository.UpdateRangeAsync(items, cancellationToken);
        return Result.Success(await HomeAdminBlocks.LoadAsync(db, images, cancellationToken));
    }
}
