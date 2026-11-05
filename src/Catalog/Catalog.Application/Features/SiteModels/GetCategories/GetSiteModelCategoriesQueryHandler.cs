using Catalog.Application.Features.SiteModels.Common;
using Catalog.Contracts.SiteModels;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.SiteModels.GetCategories;

/// <summary>
/// The category tree of «Моделі» and «Вітрина сайту» (R-18): categories shown on the site with on-site products in them
/// or below them, each with its parent, and counts that include subcategories; ordered by name within a parent.
/// </summary>
public sealed class GetSiteModelCategoriesQueryHandler(SiteModelStateReader reader)
    : IQueryHandler<GetSiteModelCategoriesQuery, Result<List<SiteModelCategoryDto>>>
{
    public async ValueTask<Result<List<SiteModelCategoryDto>>> Handle(GetSiteModelCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        var state = await reader.ReadAsync(cancellationToken);
        var counts = new Dictionary<ProductCategoryId, Counts>();

        void AddUp(ProductCategoryId categoryId, Func<Counts, Counts> change)
        {
            var visited = new HashSet<ProductCategoryId>();
            for (ProductCategoryId? current = categoryId; current is { } id && visited.Add(id);
                 current = state.CategoryParents.GetValueOrDefault(id))
            {
                counts[id] = change(counts.GetValueOrDefault(id, Counts.Zero));
            }
        }

        foreach (var product in state.Products.Values.Where(product => product.IsOnSite))
        {
            var noModel = state.ProductProblems(product.Id).Contains(SiteModelProblems.NoModel);
            AddUp(product.CategoryId, value => value with
            {
                Products = value.Products + 1, Problems = value.Problems + (noModel ? 1 : 0)
            });
        }

        foreach (var model in state.Models.Values.Where(model => state.OnSiteMembers(model.Id).Any()))
        {
            var hasProblem = model.IsAccepted && state.ModelProblems(model.Id).Count > 0;
            AddUp(model.CategoryId, value => value with
            {
                Accepted = value.Accepted + (model.IsAccepted ? 1 : 0),
                Proposed = value.Proposed + (model.IsAccepted ? 0 : 1),
                Problems = value.Problems + (hasProblem ? 1 : 0)
            });
        }

        var rows = counts
            .Where(entry => entry.Value.Products > 0 && state.ShownCategories.Contains(entry.Key))
            .Select(entry => new SiteModelCategoryDto(entry.Key.Value,
                state.CategoryNames.GetValueOrDefault(entry.Key) ?? string.Empty,
                state.CategoryParents.GetValueOrDefault(entry.Key)?.Value,
                entry.Value.Products, entry.Value.Accepted, entry.Value.Proposed, entry.Value.Problems))
            .OrderBy(row => row.Name, StringComparer.CurrentCulture)
            .ThenBy(row => row.Id)
            .ToList();

        return Result.Success(rows);
    }

    private sealed record Counts(int Products, int Accepted, int Proposed, int Problems)
    {
        public static readonly Counts Zero = new(0, 0, 0, 0);
    }
}
