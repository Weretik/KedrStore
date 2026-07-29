using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace UnitTests;

public sealed class ProductStockTests
{
    [Fact]
    public void UpdateStock_AllowsConfiguredMaximum()
    {
        var product = CreateProduct();

        product.UpdateStock(Product.MaximumStock);

        Assert.Equal(Product.MaximumStock, product.Stock);
    }

    [Fact]
    public void UpdateStock_RejectsValueAboveConfiguredMaximum()
    {
        var product = CreateProduct();

        var exception = Assert.Throws<DomainException>(() => product.UpdateStock(Product.MaximumStock + 1));

        Assert.Equal("Catalog.Product.Stock.OutOfRange", exception.Error.Code);
    }

    private static Product CreateProduct()
        => Product.Create(
            ProductId.From(1),
            "000005513",
            "Product",
            "product-1",
            ProductCategoryId.From(1),
            "https://example.test/product.jpg",
            "https://example.test/scheme.jpg",
            DateTimeOffset.UtcNow,
            0,
            1,
            false,
            false,
            true);
}
