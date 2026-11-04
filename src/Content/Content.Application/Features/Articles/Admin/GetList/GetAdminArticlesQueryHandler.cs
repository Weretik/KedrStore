using Content.Application.Contracts.MediaLibrary;
using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Admin.GetList;

public sealed class GetAdminArticlesQueryHandler(IReadContentDbContext db, IMediaImageReader images)
    : IQueryHandler<GetAdminArticlesQuery, Result<IReadOnlyList<AdminArticleListItem>>>
{
    public async ValueTask<Result<IReadOnlyList<AdminArticleListItem>>> Handle(GetAdminArticlesQuery query,
        CancellationToken cancellationToken)
    {
        var items = await db.Articles.AsNoTracking()
            .OrderByDescending(article => article.PublishedOn)
            .ThenByDescending(article => article.Id)
            .Select(article => new
            {
                article.Id, article.Slug, article.PublishedOn, article.IsPublished, article.CoverVersion, article.CoverMediaFileId, article.UpdatedAt, article.UpdatedBy,
                Title = article.Translations.Where(text => text.Language == "uk").Select(text => text.Title).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var covers = await ArticleCovers.ImagesAsync(images, items.Select(item => item.CoverMediaFileId), cancellationToken);
        return Result.Success<IReadOnlyList<AdminArticleListItem>>(items
            .Select(item => new AdminArticleListItem(item.Id.Value, item.Slug, item.Title ?? item.Slug, item.PublishedOn,
                item.IsPublished, ArticleCovers.View(item.CoverMediaFileId, item.CoverVersion, covers), item.UpdatedAt, item.UpdatedBy))
            .ToList());
    }
}
