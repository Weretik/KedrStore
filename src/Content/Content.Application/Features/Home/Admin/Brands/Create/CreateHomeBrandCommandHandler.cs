using Content.Application.Contracts.MediaLibrary;
using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Brands.Create;

public sealed class CreateHomeBrandCommandHandler(
    IContentRepository<HomeBrand> repository,
    IReadContentDbContext db,
    IMediaImageReader images,
    TimeProvider clock)
    : ICommandHandler<CreateHomeBrandCommand, Result<AdminHomeBlocks>>
{
    public async ValueTask<Result<AdminHomeBlocks>> Handle(CreateHomeBrandCommand command, CancellationToken cancellationToken)
    {
        if (await HomeAdminBlocks.CheckLogoAsync(images, command.Brand.LogoMediaFileId, cancellationToken) is { } invalid)
            return Result.Invalid(invalid);

        var position = await HomeAdminBlocks.NextBrandPositionAsync(db, cancellationToken);
        await repository.AddAsync(HomeBrand.Create(command.Brand.Name ?? string.Empty, command.Brand.LogoMediaFileId,
            position, clock.GetUtcNow()), cancellationToken);
        return Result.Success(await HomeAdminBlocks.LoadAsync(db, images, cancellationToken));
    }
}
