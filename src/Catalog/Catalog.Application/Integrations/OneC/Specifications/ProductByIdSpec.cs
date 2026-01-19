using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Specifications;

public sealed class ProductByIdSpec : Specification<Product>
{
    public ProductByIdSpec(ProductId id)
    {
        Query.Where(p => p.Id == id);
    }
}
