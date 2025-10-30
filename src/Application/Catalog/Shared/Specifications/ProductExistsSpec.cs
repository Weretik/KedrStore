using Domain.Catalog.Entities;
using Domain.Catalog.ValueObjects;

namespace Application.Catalog.Shared;

public sealed class ProductExistsSpec : Specification<Product>
{
    public ProductExistsSpec(ProductId id)
    {
        Query.Where(p => p.Id == id);
    }
}
