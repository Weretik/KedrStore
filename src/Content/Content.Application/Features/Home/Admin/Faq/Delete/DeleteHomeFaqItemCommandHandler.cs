using Content.Application.Contracts.MediaLibrary;
using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Faq.Delete;

public sealed class DeleteHomeFaqItemCommandHandler(
    IContentRepository<HomeFaqItem> repository,
    IReadContentDbContext db,
    IMediaImageReader images)
    : ICommandHandler<DeleteHomeFaqItemCommand, Result<AdminHomeBlocks>>
{
    public async ValueTask<Result<AdminHomeBlocks>> Handle(DeleteHomeFaqItemCommand command, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (item is null)
            return Result.NotFound();

        await repository.DeleteAsync(item, cancellationToken);
        return Result.Success(await HomeAdminBlocks.LoadAsync(db, images, cancellationToken));
    }
}
