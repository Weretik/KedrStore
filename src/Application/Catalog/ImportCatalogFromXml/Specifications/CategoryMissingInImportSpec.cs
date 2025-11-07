using Domain.Catalog.Entities;
using Domain.Catalog.ValueObjects;

namespace Application.Catalog.ImportCatalogFromXml;

public sealed class CategoryMissingInImportSpec: Specification<ProductCategory>
{
    public CategoryMissingInImportSpec(ProductCategoryId[] importCategoryIds)
    {
        Query.IgnoreQueryFilters()
            .Where(category => !importCategoryIds.Contains(category.Id))
            .OrderByDescending(category => category.Path.Value.Length
                                           - category.Path.Value.Replace(".", "").Length
                                           + 1);
    }
}
