using Content.Application.Contracts.MediaLibrary;
using Content.Application.Features.SeoSettings.Common;

namespace Content.Application.Features.SeoSettings.Get;

public sealed class GetSiteSeoSettingsQueryHandler(IReadContentDbContext db, IMediaImageReader images)
    : IQueryHandler<GetSiteSeoSettingsQuery, Result<SiteSeoSettingsDetails>>
{
    public async ValueTask<Result<SiteSeoSettingsDetails>> Handle(GetSiteSeoSettingsQuery query,
        CancellationToken cancellationToken)
    {
        var settings = await db.SiteSeoSettings.AsNoTracking()
            .FirstOrDefaultAsync(row => row.Id == SiteSeoSettings.SingletonId, cancellationToken);
        return settings is null
            ? Result.NotFound()
            : Result.Success(await SiteSeoSettingsMapper.ToDetailsAsync(settings, images, cancellationToken));
    }
}
