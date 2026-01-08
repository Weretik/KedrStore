using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Specifications;

public sealed class ProductsByIdsSpec : Specification<Product>
{
    public ProductsByIdsSpec(IEnumerable<ProductId> ids, bool missingOnly = false)
    {
        if (missingOnly) Query.Where(p => ids.Contains(p.Id));
        else Query.Where(p => !ids.Contains(p.Id));

    }
}
