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
        await repository.AddAsync(HomeFaqItem.Create(HomeInputs.ToDomain(command.Texts), position, clock.GetUtcNow()),
            cancellationToken);
        return Result.Success(await HomeAdminBlocks.LoadAsync(db, images, cancellationToken));
    }
}
