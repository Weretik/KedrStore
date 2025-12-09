using Application.Catalog.Shared;
using Domain.Catalog.Entities;
using Domain.Catalog.ValueObjects;

namespace Application.Catalog.GetProductById;

public sealed class GetProductByIdSpec : Specification<Product, ProductDto>
{
    public GetProductByIdSpec(int id)
    {
        Query.AsNoTracking();

        Query.Where(p => p.Id == ProductId.From(id))
            .Select(p => new ProductDto(
                p.Id.Value,
                p.Name,
                p.CategoryId.Value,
                p.ProductType.Value,
                p.Photo,
                p.Sсheme,
                p.Stock,
                p.Prices.Select(price => new ProductPriceDto(
                    price.PriceType.Name,
                    price.Price.Amount,
                    price.Price.Currency.Code)
                ).ToList())
            );
    }
}
