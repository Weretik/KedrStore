using Catalog.Domain.Entities;
using Catalog.Domain.Enumerations;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Import.Queries.ImportCatalogFromXml;

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
