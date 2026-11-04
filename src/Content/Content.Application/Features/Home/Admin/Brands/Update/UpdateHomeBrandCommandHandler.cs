using Content.Application.Contracts.MediaLibrary;
using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Brands.Update;

public sealed class UpdateHomeBrandCommandHandler(
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

        brand.Update(command.Brand.Name ?? string.Empty, command.Brand.LogoMediaFileId, clock.GetUtcNow());
        await repository.UpdateAsync(brand, cancellationToken);
        return Result.Success(await HomeAdminBlocks.LoadAsync(db, images, cancellationToken));
    }
}
