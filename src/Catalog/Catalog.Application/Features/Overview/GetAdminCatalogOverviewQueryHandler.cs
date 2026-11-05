using Catalog.Application.Contracts.Persistence;
using Catalog.Contracts.Overview;
using Catalog.Domain.Entities;

namespace Catalog.Application.Features.Overview;

/// <summary>
/// Counts products without a photo as the «Без фото» tab lists them (the list projection already leaves out deleted
/// products) and active
/// categories missing a site text in a language; a text row counts as filled the same way the category list does.
/// </summary>
public sealed class GetAdminCatalogOverviewQueryHandler(IReadCatalogDbContext catalogDbContext)
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
            .Select(category => category.Id.Value)
            .ToListAsync(cancellationToken);
        var withoutText = categoryIds.Count(id =>
            filledLanguages.GetValueOrDefault(id) < CategoryContent.Languages.Count);

        return Result.Success(new AdminCatalogOverviewResponse(withoutPhoto.Sum(row => row.Count), top, withoutText));
    }
}
