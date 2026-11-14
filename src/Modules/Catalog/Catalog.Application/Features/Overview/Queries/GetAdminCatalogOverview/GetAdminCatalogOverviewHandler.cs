using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.SiteModels.Shared;
using Catalog.Contracts.Overview;
using Catalog.Domain.Attributes;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Domain.Attributes.ValueObjects;
using Catalog.Domain.Attributes.Enums;

namespace Catalog.Application.Features.Overview.Queries.GetAdminCatalogOverview;

/// <summary>
/// Counts products without a photo as the «Без фото» tab lists them (the list projection already leaves out deleted
/// products) and active categories shown on the site (R-18) missing a site text in a language; a text row counts as
/// filled the same way the category list does. Also counts on-site products (in shown categories) whose attributes need review, and the No name ones among them (spec catalog/006),
/// and the site model health (spec catalog/007).
/// </summary>
public sealed class GetAdminCatalogOverviewHandler(
    IReadCatalogDbContext catalogDbContext,
    SiteModelStateReader siteModels)
    : IQueryHandler<GetAdminCatalogOverviewQuery, Result<AdminCatalogOverviewResponse>>
{
    public async ValueTask<Result<AdminCatalogOverviewResponse>> Handle(GetAdminCatalogOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var withoutPhoto = await catalogDbContext.ProductListProjections.AsNoTracking()
            .Where(projection => projection.Photo == string.Empty)
            .GroupBy(projection => projection.CategoryId)
            .Select(group => new { CategoryId = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        AdminCatalogOverviewCategory? top = null;
        var topRow = withoutPhoto.OrderByDescending(row => row.Count).ThenBy(row => row.CategoryId.Value).FirstOrDefault();
        if (topRow is not null)
        {
            var name = await catalogDbContext.Categories.AsNoTracking()
                .Where(category => category.Id == topRow.CategoryId)
                .Select(category => category.ShortNameUk)
                .FirstOrDefaultAsync(cancellationToken);
            top = new AdminCatalogOverviewCategory(topRow.CategoryId.Value, name ?? string.Empty, topRow.Count);
        }

        var filled = await catalogDbContext.CategoryContents.AsNoTracking()
            .Where(content => content.SeoTitle != null || content.MetaDescription != null || content.H1 != null
                              || content.BodyHtml != null || content.Faq.Any())
            .Select(content => new { content.CategoryId, content.Language })
            .ToListAsync(cancellationToken);
        var filledLanguages = filled.GroupBy(row => row.CategoryId.Value)
            .ToDictionary(group => group.Key, group => group.Select(row => row.Language).Distinct().Count());
        var categoryIds = await catalogDbContext.Categories.AsNoTracking()
            .Where(category => category.IsShownOnSite)
            .Select(category => category.Id.Value)
            .ToListAsync(cancellationToken);
        var withoutText = categoryIds.Count(id =>
            filledLanguages.GetValueOrDefault(id) < CategoryContent.Languages.Count);

        var shownCategories = catalogDbContext.Categories.AsNoTracking()
            .Where(category => category.IsShownOnSite)
            .Select(category => category.Id);
        var onSite = catalogDbContext.Products.AsNoTracking()
            .Where(product => product.ExportToSite && shownCategories.Contains(product.CategoryId))
            .Select(product => product.Id);
        var problems = await catalogDbContext.ProductAttributeReviews.AsNoTracking()
            .Where(review => review.State == AttributeReviewState.NeedsReview && onSite.Contains(review.ProductId))
            .Select(review => review.Reasons)
            .ToListAsync(cancellationToken);
        var noName = problems.Count(reasons => reasons.Any(reason => reason.Code == AttributeReviewReason.NoName));

        var health = (await siteModels.ReadAsync(cancellationToken)).Health();

        return Result.Success(new AdminCatalogOverviewResponse(withoutPhoto.Sum(row => row.Count), top, withoutText,
            problems.Count, noName, health));
    }
}
