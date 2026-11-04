using Content.Application.Contracts.MediaLibrary;
using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Public.GetBySlug;

public sealed class GetPublicArticleQueryHandler(IReadContentDbContext db, IMediaImageReader images)
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
                article.Id, article.Slug, article.PublishedOn, article.CoverVersion, article.CoverMediaFileId, article.UpdatedAt,
                Text = article.Translations.Where(text => text.Language == language)
                    .Select(text => new { text.Title, text.Excerpt, text.BodyHtml }).FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (article?.Text is null)
            return Result.NotFound();

        var covers = await ArticleCovers.ImagesAsync(images, [article.CoverMediaFileId], cancellationToken);
        return Result.Success(new PublicArticleDetails(article.Id.Value, article.Slug, article.Text.Title,
            article.Text.Excerpt, article.Text.BodyHtml, article.PublishedOn,
            ArticleCovers.View(article.CoverMediaFileId, article.CoverVersion, covers), article.UpdatedAt));
    }
}
