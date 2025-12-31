using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Shared;

public sealed class ProductExistsSpec : Specification<Product>
{
    public ProductExistsSpec(ProductId id)
    {
        Query.Where(p => p.Id == id);
    }
}
