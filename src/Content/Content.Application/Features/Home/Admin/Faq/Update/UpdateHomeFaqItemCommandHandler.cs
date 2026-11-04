using Content.Application.Contracts.MediaLibrary;
using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Faq.Update;

public sealed class UpdateHomeFaqItemCommandHandler(
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

        item.Update(HomeInputs.ToDomain(command.Texts), clock.GetUtcNow());
        await repository.UpdateAsync(item, cancellationToken);
        return Result.Success(await HomeAdminBlocks.LoadAsync(db, images, cancellationToken));
    }
}
