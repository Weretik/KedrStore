namespace Content.Application.Features.Overview.Queries.GetAdminContentOverview;

public sealed class GetAdminContentOverviewHandler(IReadContentDbContext db)
    : IQueryHandler<GetAdminContentOverviewQuery, Result<AdminContentOverview>>
{
    public async ValueTask<Result<AdminContentOverview>> Handle(GetAdminContentOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var articles = db.Articles.AsNoTracking();
        var drafts = await articles.CountAsync(article => !article.IsPublished, cancellationToken);
        var oldest = await articles.Where(article => !article.IsPublished)
            .OrderBy(article => article.UpdatedAt).ThenBy(article => article.Id)
            .Select(article => new
            {
                article.Id, article.Slug, article.UpdatedAt,
                Title = article.Translations.Where(text => text.Language == "uk").Select(text => text.Title).FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);
        var legacyCovers = await articles.CountAsync(article => article.CoverVersion != null, cancellationToken);
        var documentDrafts = await db.LegalDocumentVersions.AsNoTracking()
            .CountAsync(version => version.Status == LegalDocumentStatus.Draft, cancellationToken);

        return Result.Success(new AdminContentOverview(drafts,
            oldest is null ? null : new AdminContentOverviewDraft(oldest.Id.Value, oldest.Title ?? oldest.Slug, oldest.UpdatedAt),
            legacyCovers, documentDrafts));
    }
}
