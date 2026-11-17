using Content.Application.Abstractions.Media;
using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.DeleteHomeFaqItem;

public sealed class DeleteHomeFaqItemHandler(
    IContentRepository<HomeFaqItem> repository,
    IReadContentDbContext db,
    IMediaImageReader images)
    : ICommandHandler<DeleteHomeFaqItemCommand, Result<AdminHomeBlocks>>
{
    public async ValueTask<Result<AdminHomeBlocks>> Handle(DeleteHomeFaqItemCommand command, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(HomeFaqItemId.FromStorage(command.Id), cancellationToken);
        if (item is null)
            return Result.NotFound();

        await repository.DeleteAsync(item, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success(await HomeAdminBlocks.LoadAsync(db, images, cancellationToken));
    }
}
