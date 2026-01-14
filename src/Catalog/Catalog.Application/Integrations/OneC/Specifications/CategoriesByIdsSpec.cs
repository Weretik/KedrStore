using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Specifications;

public sealed class CategoriesByIdsSpec : Specification<ProductCategory>
{
    public CategoriesByIdsSpec(IEnumerable<ProductCategoryId> ids, string productTypeIdOneC, bool missingOnly = false)
    {
        if (missingOnly) Query.Where(c => !ids.Contains(c.Id) && c.ProductTypeIdOneC == productTypeIdOneC);
        else Query.Where(c => ids.Contains(c.Id) && c.ProductTypeIdOneC == productTypeIdOneC);
    }
}
