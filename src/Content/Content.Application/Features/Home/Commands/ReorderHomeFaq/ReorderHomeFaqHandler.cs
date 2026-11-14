using Content.Application.Abstractions.Media;
using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.ReorderHomeFaq;

/// <summary>Saves the whole order; a list built from a stale page answers 409 instead of losing a row.</summary>
public sealed class ReorderHomeFaqHandler(
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
            return Result.Conflict("Список змінився, оновіть сторінку й спробуйте ще раз.");

        await repository.UpdateRangeAsync(items, cancellationToken);
        return Result.Success(await HomeAdminBlocks.LoadAsync(db, images, cancellationToken));
    }
}
