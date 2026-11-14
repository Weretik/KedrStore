using Content.Application.Abstractions.Media;
using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.UpdateHomeBrand;

public sealed class UpdateHomeBrandHandler(
    IContentRepository<HomeBrand> repository,
    IReadContentDbContext db,
    IMediaImageReader images,
    TimeProvider clock)
    : ICommandHandler<UpdateHomeBrandCommand, Result<AdminHomeBlocks>>
{
    public async ValueTask<Result<AdminHomeBlocks>> Handle(UpdateHomeBrandCommand command, CancellationToken cancellationToken)
    {
        var brand = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (brand is null)
            return Result.NotFound();
        if (brand.LogoMediaFileId != command.Brand.LogoMediaFileId &&
            await HomeAdminBlocks.CheckLogoAsync(images, command.Brand.LogoMediaFileId, cancellationToken) is { } invalid)
            return Result.Invalid(invalid);

        var updated = brand.Update(command.Brand.Name ?? string.Empty, command.Brand.LogoMediaFileId, clock.GetUtcNow());
        if (updated.IsFailure)
            return updated.Error!.ToResult<AdminHomeBlocks>();
        await repository.UpdateAsync(brand, cancellationToken);
        return Result.Success(await HomeAdminBlocks.LoadAsync(db, images, cancellationToken));
    }
}
