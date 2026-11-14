using Content.Application.Abstractions.Media;
using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.DeleteHomeBrand;

public sealed class DeleteHomeBrandHandler(
    IContentRepository<HomeBrand> repository,
    IReadContentDbContext db,
    IMediaImageReader images)
    : ICommandHandler<DeleteHomeBrandCommand, Result<AdminHomeBlocks>>
{
    public async ValueTask<Result<AdminHomeBlocks>> Handle(DeleteHomeBrandCommand command, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (item is null)
            return Result.NotFound();

        await repository.DeleteAsync(item, cancellationToken);
        return Result.Success(await HomeAdminBlocks.LoadAsync(db, images, cancellationToken));
    }
}
