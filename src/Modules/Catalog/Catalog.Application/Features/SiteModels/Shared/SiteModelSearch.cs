using Catalog.Application.Features.SiteModels.Shared.State;

namespace Catalog.Application.Features.SiteModels.Shared;

/// <summary>
/// The admin model search over the loaded state (database rules 2.1, applied in memory): every word of the search is
/// part of the model name or of a product name, or is a product number.
/// </summary>
internal static class SiteModelSearch
{
    public static bool Matches(SiteModelState state, string name, IReadOnlyCollection<ProductId> products, string search)
        => search.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .All(word => name.Contains(word, StringComparison.CurrentCultureIgnoreCase) ||
                         products.Any(id => id.Value.ToString() == word ||
                                            state.Products[id].Name.Contains(word,
                                                StringComparison.CurrentCultureIgnoreCase)));
}
