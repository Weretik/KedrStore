using Content.Application.Abstractions.Media;
using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.CreateHomeFaqItem;

public sealed class CreateHomeFaqItemHandler(
    IContentRepository<HomeFaqItem> repository,
    IReadContentDbContext db,
    IMediaImageReader images,
    TimeProvider clock)
    : ICommandHandler<CreateHomeFaqItemCommand, Result<AdminHomeBlocks>>
{
    public async ValueTask<Result<AdminHomeBlocks>> Handle(CreateHomeFaqItemCommand command, CancellationToken cancellationToken)
    {
        var position = await HomeAdminBlocks.NextFaqPositionAsync(db, cancellationToken);
        var item = HomeFaqItem.Create(HomeInputs.ToDomain(command.Texts), position, clock.GetUtcNow());
        if (item.IsFailure)
            return item.Error!.ToResult<AdminHomeBlocks>();
        await repository.AddAsync(item.Value, cancellationToken);
        return Result.Success(await HomeAdminBlocks.LoadAsync(db, images, cancellationToken));
    }
}
