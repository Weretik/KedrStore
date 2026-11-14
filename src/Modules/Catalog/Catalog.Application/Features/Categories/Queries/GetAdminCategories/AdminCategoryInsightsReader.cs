using Catalog.Application.Abstractions.Persistence;

namespace Catalog.Application.Features.Categories.Queries.GetAdminCategories;

/// <summary>
/// Reads the list projection (already without deleted products and products of deleted categories) and the stored
/// category texts. Empty fields are stored as <c>null</c>, so a row with any value or FAQ item counts as filled.
/// </summary>
public sealed class AdminCategoryInsightsReader(IReadCatalogDbContext catalogDbContext) : IAdminCategoryInsightsReader
{
    public async Task<IReadOnlyDictionary<int, int>> LoadDirectProductCountsAsync(CancellationToken cancellationToken)
    {
        var rows = await catalogDbContext.ProductListProjections.AsNoTracking()
            .GroupBy(projection => projection.CategoryId)
            .Select(group => new { CategoryId = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(row => row.CategoryId.Value, row => row.Count);
    }

    public async Task<IReadOnlyDictionary<int, IReadOnlyList<string>>> LoadFilledContentLanguagesAsync(
        CancellationToken cancellationToken)
    {
        var rows = await catalogDbContext.CategoryContents.AsNoTracking()
            .Where(content => content.SeoTitle != null || content.MetaDescription != null || content.H1 != null
                              || content.BodyHtml != null || content.Faq.Any())
            .Select(content => new { content.CategoryId, content.Language })
            .ToListAsync(cancellationToken);

        return rows.GroupBy(row => row.CategoryId.Value)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group.Select(row => row.Language).Order(StringComparer.Ordinal).ToList());
    }
}
