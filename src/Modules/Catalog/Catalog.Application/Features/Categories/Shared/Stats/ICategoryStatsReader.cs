namespace Catalog.Application.Features.Categories.Shared.Stats;

public interface ICategoryStatsReader
{
    /// <summary>Exported products and retail price range per category, without descendants.</summary>
    Task<IReadOnlyDictionary<int, CategoryStats>> LoadDirectAsync(CancellationToken cancellationToken);
}
