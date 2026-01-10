using Catalog.Application.Features.Shared;
using Catalog.Application.Shared;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdSpec : Specification<Product, ProductDto>
{
    public GetProductByIdSpec(int id)
    {
        Query.AsNoTracking();

        Query.Where(p => p.Id == ProductId.From(id))
            .Select(p => new ProductDto(
                Id: p.Id.Value,
                Name: p.Name,
                CategoryId: p.CategoryId.Value,
                Photo: p.Photo,
                Scheme: p.Sсheme,
                Stock: p.Stock,
                IsSale: p.IsSale,
                IsNew: p.IsNew,
                QuantityInPack: p.QuantityInPack)
            );
    }
}
