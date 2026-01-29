using Catalog.Application.Integrations.OneC.Mappers;
using Xunit;
using Catalog.Application.Integrations.OneC.DTOs;
using System.Collections.Generic;

namespace UnitTests;

public class SlugTests
{
    [Fact]
    public void MapProduct_Should_Clean_Slugs()
    {
        var oneCProduct = new OneCProductDto(
            Id: 7914,
            Name: ".-1919-b-black-glukha-ruchka-do-protipozhezh.-zamku",
            CategoryPath: "Test",
            Manufacturer: "Test",
            IsSale: false,
            IsNew: false,
            ExportToSite: true,
            QuantityInPack: 1
        );

        var products = CatalogMapper.MapProduct(
            new List<OneCProductDto> { oneCProduct },
            new Dictionary<string, int> { { "Test", 1 } },
            "Root"
        );

        var slug = products[0].ProducSlug;
        System.Console.WriteLine($"[DEBUG_LOG] Generated Slug: {slug}");

        Assert.False(slug.StartsWith('.'));
        Assert.DoesNotContain("..", slug);
        Assert.False(slug.EndsWith('.'));

        var nameWithDotInMiddle = "Product.With.Dot";
        var oneCProduct2 = oneCProduct with { Name = nameWithDotInMiddle };
        var products2 = CatalogMapper.MapProduct(
            new List<OneCProductDto> { oneCProduct2 },
            new Dictionary<string, int> { { "Test", 1 } },
            "Root"
        );
        var slug2 = products2[0].ProducSlug;
        System.Console.WriteLine($"[DEBUG_LOG] Generated Slug 2: {slug2}");
        Assert.DoesNotContain(".", slug2);
    }
}
