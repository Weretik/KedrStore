using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Public.GetBySlug;

public sealed class GetPublicArticleQueryHandler(IReadContentDbContext db)
    : IQueryHandler<GetPublicArticleQuery, Result<PublicArticleDetails>>
{
    public async ValueTask<Result<PublicArticleDetails>> Handle(GetPublicArticleQuery query,
        CancellationToken cancellationToken)
    {
        var language = query.Language;
        var slug = query.Slug;
        var article = await db.Articles.AsNoTracking()
            .Where(article => article.IsPublished && article.Slug == slug)
            .Select(article => new
            {
                article.Id, article.Slug, article.PublishedOn, article.CoverVersion, article.UpdatedAt,
                Text = article.Translations.Where(text => text.Language == language)
                    .Select(text => new { text.Title, text.Excerpt, text.BodyHtml }).FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (article?.Text is null)
            return Result.NotFound();

        return Result.Success(new PublicArticleDetails(article.Id.Value, article.Slug, article.Text.Title,
            article.Text.Excerpt, article.Text.BodyHtml, article.PublishedOn, article.CoverVersion, article.UpdatedAt));
    }
}
