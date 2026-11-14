using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Articles.Shared.Reading;

namespace Content.Application.Features.Articles.Queries.GetArticleCover;

public sealed class GetArticleCoverHandler(IReadContentDbContext db)
    : IQueryHandler<GetArticleCoverQuery, Result<ArticleCoverFile>>
{
    public async ValueTask<Result<ArticleCoverFile>> Handle(GetArticleCoverQuery query, CancellationToken cancellationToken)
    {
        var id = ArticleId.Create(query.ArticleId);
        var version = await db.Articles.AsNoTracking()
            .Where(article => article.Id == id)
            .Select(article => article.CoverVersion)
            .FirstOrDefaultAsync(cancellationToken);
        if (version is null)
            return Result.NotFound();

        var cover = await db.ArticleCovers.AsNoTracking()
            .Where(cover => cover.ArticleId == id)
            .Select(cover => new { cover.Data, cover.ContentType })
            .FirstOrDefaultAsync(cancellationToken);

        return cover is null
            ? Result.NotFound()
            : Result.Success(new ArticleCoverFile(cover.Data, cover.ContentType, version.Value));
    }
}
