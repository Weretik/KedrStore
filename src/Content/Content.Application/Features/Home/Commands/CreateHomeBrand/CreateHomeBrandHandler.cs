using Content.Application.Abstractions.Media;
using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.CreateHomeBrand;

public sealed class CreateHomeBrandHandler(
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
        var brand = HomeBrand.Create(command.Brand.Name ?? string.Empty, command.Brand.LogoMediaFileId, position,
            clock.GetUtcNow());
        if (brand.IsFailure)
            return brand.Error!.ToResult<AdminHomeBlocks>();
        await repository.AddAsync(brand.Value, cancellationToken);
        return Result.Success(await HomeAdminBlocks.LoadAsync(db, images, cancellationToken));
    }
}
