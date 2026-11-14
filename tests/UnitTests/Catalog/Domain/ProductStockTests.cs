using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Categories;
using Catalog.Domain.Categories.Content;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Slugs;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace UnitTests.Catalog.Domain;

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
            DateTimeOffset.UtcNow,
            0,
            1,
            false,
            false,
            true);
}
