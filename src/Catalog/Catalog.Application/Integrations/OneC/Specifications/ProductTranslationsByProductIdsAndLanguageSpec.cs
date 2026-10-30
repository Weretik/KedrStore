using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Specifications;

/// <summary>Translations of the products in one language, including soft-deleted ones, so they can be revived.</summary>
public sealed class ProductTranslationsByProductIdsAndLanguageSpec : Specification<ProductTranslation>
{
    public ProductTranslationsByProductIdsAndLanguageSpec(IEnumerable<ProductId> productIds, string language)
    {
        Query
            .IgnoreQueryFilters()
            .Where(x => productIds.Contains(x.ProductId) && x.Language == language);
    }
}
