using Catalog.Domain.Entities;

namespace Catalog.Application.Integrations.OneC.Specifications;

/// <summary>Every category of a 1C root, including soft-deleted ones, tracked for the import diff.</summary>
public sealed class CategoriesByRootSpec : Specification<ProductCategory>
{
    public CategoriesByRootSpec(string productTypeIdOneC)
    {
        Query
            .IgnoreQueryFilters()
            .Where(category => category.ProductTypeIdOneC == productTypeIdOneC);
    }
}
