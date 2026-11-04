using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Admin.GetById;

public sealed class GetAdminArticleQueryHandler(IReadContentDbContext db)
    : IQueryHandler<GetAdminArticleQuery, Result<AdminArticleDetails>>
{
    public async ValueTask<Result<AdminArticleDetails>> Handle(GetAdminArticleQuery query, CancellationToken cancellationToken)
    {
        var id = ArticleId.Create(query.ArticleId);
        var article = await db.Articles.AsNoTracking().FirstOrDefaultAsync(article => article.Id == id, cancellationToken);
        return article is null ? Result.NotFound() : Result.Success(ArticleMapper.ToAdmin(article));
    }
}
