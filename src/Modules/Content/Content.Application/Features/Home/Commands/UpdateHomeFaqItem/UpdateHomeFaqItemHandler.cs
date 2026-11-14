using Content.Application.Abstractions.Media;
using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.UpdateHomeFaqItem;

public sealed class UpdateHomeFaqItemHandler(
    IContentRepository<HomeFaqItem> repository,
    IReadContentDbContext db,
    IMediaImageReader images,
    TimeProvider clock)
    : ICommandHandler<UpdateHomeFaqItemCommand, Result<AdminHomeBlocks>>
{
    public async ValueTask<Result<AdminHomeBlocks>> Handle(UpdateHomeFaqItemCommand command, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (item is null)
            return Result.NotFound();

        var updated = item.Update(HomeInputs.ToDomain(command.Texts), clock.GetUtcNow());
        if (updated.IsFailure)
            return updated.Error!.ToResult<AdminHomeBlocks>();
        await repository.UpdateAsync(item, cancellationToken);
        return Result.Success(await HomeAdminBlocks.LoadAsync(db, images, cancellationToken));
    }
}
