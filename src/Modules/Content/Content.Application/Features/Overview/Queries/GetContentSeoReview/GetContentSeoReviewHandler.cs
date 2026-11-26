namespace Content.Application.Features.Overview.Queries.GetContentSeoReview;

public sealed class GetContentSeoReviewHandler(IReadContentDbContext db)
    : IQueryHandler<GetContentSeoReviewQuery, Result<IReadOnlyList<ContentSeoReviewItem>>>
{
    public async ValueTask<Result<IReadOnlyList<ContentSeoReviewItem>>> Handle(GetContentSeoReviewQuery query,
        CancellationToken cancellationToken)
    {
        var texts = await db.Articles.AsNoTracking()
            .Where(article => article.IsPublished)
            .SelectMany(article => article.Translations.Select(text => new
            {
                article.Id, text.Language, text.Title, text.SeoTitle, text.MetaDescription
            }))
            .ToListAsync(cancellationToken);

        var pages = texts.Select(text => new ContentSeoPage(text.Id.Value, text.Title, text.Language,
                string.IsNullOrWhiteSpace(text.SeoTitle) ? text.Title : text.SeoTitle, text.MetaDescription))
            .OrderBy(page => page.Language).ThenBy(page => page.Id)
            .ToList();
        return Result.Success(ContentSeoChecks.Review(pages));
    }
}
