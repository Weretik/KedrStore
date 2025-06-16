namespace Domain.Catalog.Specifications;

public class PriceRangeSpecification(decimal? minPrice, decimal? maxPrice)
    : SpecificationBase<Product>
{
    public override Expression<Func<Product, bool>> ToExpression()
    {
        return product => (!minPrice.HasValue || product.Price.Amount >= minPrice.Value)
                          &&
                         (!maxPrice.HasValue || product.Price.Amount <= maxPrice.Value);
    }
}
