using Domain.Catalog.Entities;
using Domain.Catalog.ValueObjects;

namespace Application.Catalog.Shared;

public sealed class CategoryExistsSpec : Specification<ProductCategory>
{
    public CategoryExistsSpec(ProductCategoryId id)
    {
        Query.Where(c => c.Id == id);
    }
}
