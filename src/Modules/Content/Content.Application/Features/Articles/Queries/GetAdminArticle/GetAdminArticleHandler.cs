using Content.Application.Abstractions.Media;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetAdminArticle;

public sealed class GetAdminArticleHandler(IReadContentDbContext db, IMediaImageReader images)
    : IQueryHandler<GetAdminArticleQuery, Result<AdminArticleDetails>>
{
    public async ValueTask<Result<AdminArticleDetails>> Handle(GetAdminArticleQuery query, CancellationToken cancellationToken)
    {
        var id = ArticleId.Create(query.ArticleId);
        var article = await db.Articles.AsNoTracking().FirstOrDefaultAsync(article => article.Id == id, cancellationToken);
        if (article is null)
            return Result.NotFound();
        return Result.Success(ArticleMapper.ToAdmin(article, await ArticleCovers.ViewAsync(images, article, cancellationToken)));
    }
}
