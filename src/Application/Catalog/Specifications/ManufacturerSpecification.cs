namespace Domain.Catalog.Specifications;

public class ManufacturerSpecification(string? manufacturer)
    : SpecificationBase<Product>
{
    private readonly string? _manufacturer = manufacturer?.Trim().ToLower();

    public override Expression<Func<Product, bool>> ToExpression()
    {
        return product => string.IsNullOrEmpty(_manufacturer) ||
                         product.Manufacturer.ToLower().Contains(_manufacturer);
    }
}
