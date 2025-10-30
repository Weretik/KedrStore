using Domain.Catalog.Entities;

namespace Application.Catalog.ImportCatalogFromXml;

public sealed class CategoryMissingInImportSpec: Specification<ProductCategory>
{
    public CategoryMissingInImportSpec(ISet<int> importIds)
    {
        Query.IgnoreQueryFilters()
            .Where(c => !importIds.Contains(c.Id.Value))
            .OrderByDescending(c => c.Path.NLevel);
    }
}
