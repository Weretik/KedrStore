using Content.Application.Abstractions.Media;
using Content.Application.Features.SeoSettings.Shared;

namespace Content.Application.Features.SeoSettings.Queries.GetSiteSeoSettings;

public sealed class GetSiteSeoSettingsHandler(IReadContentDbContext db, IMediaImageReader images)
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
