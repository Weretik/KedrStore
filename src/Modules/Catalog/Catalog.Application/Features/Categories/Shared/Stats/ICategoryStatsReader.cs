namespace Catalog.Application.Features.Categories.Shared.Stats;

public interface ICategoryStatsReader
{
    /// <summary>Exported products and retail price range per category, without descendants.</summary>
    Task<IReadOnlyDictionary<int, CategoryStats>> LoadDirectAsync(CancellationToken cancellationToken);

    /// <summary>When each category's content in the language last changed (spec catalog/011 R-04).</summary>
    Task<IReadOnlyDictionary<int, DateTimeOffset>> LoadContentChangesAsync(string language,
        CancellationToken cancellationToken);
}
