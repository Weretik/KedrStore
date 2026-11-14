using Content.Application.Abstractions.Media;
using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Queries.GetPublicHomeBlocks;

/// <summary>Home page FAQ and brands in one language; a brand whose logo file was deleted is left out.</summary>
public sealed class GetPublicHomeBlocksHandler(IReadContentDbContext db, IMediaImageReader images)
    : IQueryHandler<GetPublicHomeBlocksQuery, Result<PublicHomeBlocks>>
{
    public async ValueTask<Result<PublicHomeBlocks>> Handle(GetPublicHomeBlocksQuery query,
        CancellationToken cancellationToken)
    {
        var uk = query.Language == "uk";
        var faq = await db.HomeFaqItems.AsNoTracking()
            .OrderBy(item => item.Position).ThenBy(item => item.Id)
            .Select(item => new PublicHomeFaqItem(uk ? item.QuestionUk : item.QuestionRu,
                uk ? item.AnswerUk : item.AnswerRu))
            .ToListAsync(cancellationToken);
        var brands = await db.HomeBrands.AsNoTracking()
            .OrderBy(brand => brand.Position).ThenBy(brand => brand.Id)
            .Select(brand => new { brand.Name, brand.LogoMediaFileId })
            .ToListAsync(cancellationToken);
        var logos = await images.GetImagesAsync(brands.Select(brand => brand.LogoMediaFileId).ToList(),
            cancellationToken);

        var visible = brands
            .Where(brand => logos.ContainsKey(brand.LogoMediaFileId))
            .Select(brand =>
            {
                var logo = logos[brand.LogoMediaFileId];
                return new PublicHomeBrand(brand.Name, logo.Url, logo.Width, logo.Height);
            })
            .ToList();
        return Result.Success(new PublicHomeBlocks(faq, visible));
    }
}
