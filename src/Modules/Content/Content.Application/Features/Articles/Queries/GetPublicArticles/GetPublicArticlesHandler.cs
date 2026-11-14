using Content.Application.Abstractions.Media;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Articles.Shared.Reading;
using Content.Application.Features.Articles.Shared.Writing;

namespace Content.Application.Features.Articles.Queries.GetPublicArticles;

public sealed class GetPublicArticlesHandler(IReadContentDbContext db, IMediaImageReader images)
    : IQueryHandler<GetPublicArticlesQuery, Result<IReadOnlyList<PublicArticleListItem>>>
{
    public async ValueTask<Result<IReadOnlyList<PublicArticleListItem>>> Handle(GetPublicArticlesQuery query,
        CancellationToken cancellationToken)
    {
        var language = query.Language;
        var items = await db.Articles.AsNoTracking()
            .Where(article => article.IsPublished)
            .OrderByDescending(article => article.PublishedOn)
            .ThenByDescending(article => article.Id)
            .Select(article => new
            {
                article.Id, article.Slug, article.PublishedOn, article.CoverVersion, article.CoverMediaFileId, article.UpdatedAt,
                Text = article.Translations.Where(text => text.Language == language)
                    .Select(text => new { text.Title, text.Excerpt }).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var covers = await ArticleCovers.ImagesAsync(images, items.Select(item => item.CoverMediaFileId), cancellationToken);
        return Result.Success<IReadOnlyList<PublicArticleListItem>>(items
            .Where(item => item.Text is not null)
            .Select(item => new PublicArticleListItem(item.Id.Value, item.Slug, item.Text!.Title, item.Text.Excerpt,
                item.PublishedOn, ArticleCovers.View(item.CoverMediaFileId, item.CoverVersion, covers), item.UpdatedAt))
            .ToList());
    }
}
