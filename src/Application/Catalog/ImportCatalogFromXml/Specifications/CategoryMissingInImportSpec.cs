using Domain.Catalog.Entities;
using Domain.Catalog.Enumerations;
using Domain.Catalog.ValueObjects;

namespace Application.Catalog.ImportCatalogFromXml;

public sealed class CategoryMissingInImportSpec: Specification<ProductCategory>
{
    public CategoryMissingInImportSpec(ProductCategoryId[] importCategoryIds, int productTypeId)
    {
        Query.IgnoreQueryFilters()
            .Where(category =>
                category.ProductType == ProductType.FromValue(productTypeId)
                && !importCategoryIds.Contains(category.Id))
            .OrderByDescending(p => EF.Property<string>(p, "Path"));
    }
}
