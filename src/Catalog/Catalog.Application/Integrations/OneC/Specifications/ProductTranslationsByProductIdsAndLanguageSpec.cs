using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Specifications;

public sealed class ProductTranslationsByProductIdsAndLanguageSpec : Specification<ProductTranslation>
{
    public ProductTranslationsByProductIdsAndLanguageSpec(IEnumerable<ProductId> productIds, string language)
    {
        Query.Where(x => productIds.Contains(x.ProductId) && x.Language == language);
    }
}
