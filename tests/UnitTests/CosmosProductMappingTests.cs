using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Integrations.OneC.Mappers;

namespace UnitTests;

public sealed class CosmosProductMappingTests
{
    [Fact]
    public void MapProduct_UsesConfiguredFallbackCategory_ForCosmosRoot()
    {
        var source = new[]
        {
            new OneCProductDto(42, "Cosmos product", "Ignored 1C category", "", false, false, true, 1)
        };

        var result = CatalogMapper.MapProduct(source, [], "Космос", fallbackCategoryId: 920000);

        var item = Assert.Single(result);
        Assert.Equal(920000, item.CategoryId);
        Assert.Equal("Космос", item.ProductTypeIdOneC);
    }

    [Fact]
    public void MapProduct_DeduplicatesRepeatedOneCProductIds()
    {
        var product = new OneCProductDto(
            26358, "Product", "Ignored", "", false, false, true, 1);

        var mapped = CatalogMapper.MapProduct(
            [product, product with { Name = "Repeated product" }],
            new Dictionary<string, int>(),
            "Космос",
            fallbackCategoryId: 920000);

        var item = Assert.Single(mapped);
        Assert.Equal(26358, item.Id);
        Assert.Equal("Product", item.Name);
    }
}
