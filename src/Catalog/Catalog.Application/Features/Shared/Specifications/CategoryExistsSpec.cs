using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Shared;

public sealed class CategoryExistsSpec : Specification<ProductCategory>
{
    public CategoryExistsSpec(ProductCategoryId id)
    {
        Query.Where(c => c.Id == id);
    }
}
