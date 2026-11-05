using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.SiteModels.Common;
using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.GetCategories;

/// <summary>
/// Final categories (without children) that have on-site products or models, with their counts, for the «Моделі»
/// sidebar; ordered by parent name, then name.
/// </summary>
public sealed class GetSiteModelCategoriesQueryHandler(IReadCatalogDbContext db, SiteModelStateReader reader)
    : IQueryHandler<GetSiteModelCategoriesQuery, Result<List<SiteModelCategoryDto>>>
{
    public async ValueTask<Result<List<SiteModelCategoryDto>>> Handle(GetSiteModelCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        var categories = await db.Categories.AsNoTracking()
            .Select(category => new { category.Id, category.ShortNameUk, category.Name, category.ParentId })
            .ToListAsync(cancellationToken);
        var parents = categories.Where(category => category.ParentId is not null)
            .Select(category => category.ParentId!.Value).ToHashSet();
        var names = categories.ToDictionary(category => category.Id,
            category => string.IsNullOrWhiteSpace(category.ShortNameUk) ? category.Name : category.ShortNameUk);
        var state = await reader.ReadAsync(cancellationToken);

        var onSite = state.Products.Values.Where(product => product.IsOnSite).ToLookup(product => product.CategoryId);
        var models = state.Models.Values
            .Where(model => state.MembersByModel[model.Id].Any(id => !state.Products[id].IsDeleted))
            .ToLookup(model => model.CategoryId);

        var rows = categories
            .Where(category => !parents.Contains(category.Id))
            .Where(category => onSite[category.Id].Any() || models[category.Id].Any())
            .Select(category =>
            {
                var accepted = models[category.Id].Where(model => model.IsAccepted).ToList();
                var problems = accepted.Count(model => state.ModelProblems(model.Id).Count > 0) +
                               onSite[category.Id].Count(product =>
                                   state.ProductProblems(product.Id).Contains(SiteModelProblems.NoModel));
                return new SiteModelCategoryDto(category.Id.Value, names[category.Id],
                    category.ParentId is { } parentId ? names.GetValueOrDefault(parentId) : null,
                    onSite[category.Id].Count(), accepted.Count, models[category.Id].Count() - accepted.Count, problems);
            })
            .OrderBy(row => row.ParentName, StringComparer.CurrentCulture)
            .ThenBy(row => row.Name, StringComparer.CurrentCulture)
            .ToList();

        return Result.Success(rows);
    }
}
