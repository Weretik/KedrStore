namespace Catalog.Application.Features.Categories.Queries.GetAdminCategories;

/// <summary>What the admin category list shows beside each row: how many products and models and which texts exist.</summary>
public interface IAdminCategoryInsightsReader
{
    /// <summary>Active products per category, without descendants, whether or not they are on the site.</summary>
    Task<IReadOnlyDictionary<int, int>> LoadDirectProductCountsAsync(CancellationToken cancellationToken);

    /// <summary>Site models whose site category is this one, without descendants; categories without models are left out.</summary>
    Task<IReadOnlyDictionary<int, CategoryModelCounts>> LoadDirectModelCountsAsync(CancellationToken cancellationToken);

    /// <summary>Per category, the languages that have at least one site text field or FAQ item.</summary>
    Task<IReadOnlyDictionary<int, IReadOnlyList<string>>> LoadFilledContentLanguagesAsync(CancellationToken cancellationToken);
}
