using Content.Application.Contracts.MediaLibrary;
using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Faq.Create;

public sealed class CreateHomeFaqItemCommandHandler(
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
