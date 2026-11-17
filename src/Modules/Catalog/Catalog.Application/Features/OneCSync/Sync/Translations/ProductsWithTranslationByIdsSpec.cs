namespace Catalog.Application.Features.OneCSync.Sync.Translations;

/// <summary>
/// Live products with their translation in one language, including a soft-deleted one, so the product can revive it
/// instead of adding a duplicate.
/// </summary>
public sealed class ProductsWithTranslationByIdsSpec : Specification<Product>
{
    public ProductsWithTranslationByIdsSpec(IEnumerable<ProductId> productIds, string language)
    {
        Query
            .IgnoreQueryFilters()
            .Where(product => productIds.Contains(product.Id) && !product.IsDeleted)
            .Include(product => product.Translations.Where(translation => translation.Language == language));
    }
}
