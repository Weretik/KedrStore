using Catalog.Application.Jobs.Slugs;
using Catalog.Application.Slugs;
using Catalog.Domain.ValueObjects;

namespace UnitTests.Catalog.Slugs;

public sealed class SlugTextTests
{
    [Theory]
    [InlineData("Замки", "zamky")]
    [InlineData("Ручки на розетці", "ruchky-na-rozettsi")]
    [InlineData("Завіси врізні", "zavisy-vrizni")]
    [InlineData("Згорани", "zghorany")]
    [InlineData("Юрій", "yurii")]
    [InlineData("Їжак", "yizhak")]
    [InlineData("Знам'янка", "znamianka")]
    [InlineData("Щеколда", "shchekolda")]
    [InlineData("Яблуко і єнот", "yabluko-i-yenot")]
    [InlineData("Подвійний ґудзик", "podviinyi-gudzyk")]
    [InlineData("Ручка R-100, чорний матовий", "ruchka-r-100-chornyi-matovyi")]
    [InlineData(".-1919-b-black-glukha-ruchka-do-protipozhezh.-zamku", "1919-b-black-glukha-ruchka-do-protipozhezh-zamku")]
    [InlineData("Product.With.Dot", "product-with-dot")]
    public void FromName_UsesKmu2010AndSingleHyphens(string name, string expected)
        => Assert.Equal(expected, SlugText.FromName(name, CatalogEntityType.Product));

    [Theory]
    [InlineData("", CatalogEntityType.Category, "category")]
    [InlineData("!!!", CatalogEntityType.Product, "product")]
    [InlineData(null, CatalogEntityType.Product, "product")]
    public void FromName_FallsBackForEmptyResult(string? name, CatalogEntityType kind, string expected)
        => Assert.Equal(expected, SlugText.FromName(name, kind));

    [Fact]
    public void FromName_CutsAtWordBoundaryWithinMaxLength()
    {
        var name = string.Join(' ', Enumerable.Repeat("довга-назва", 20));

        var slug = SlugText.FromName(name, CatalogEntityType.Product);

        Assert.True(slug.Length <= SlugText.MaxLength);
        Assert.True(SlugText.IsValid(slug));
        Assert.StartsWith(slug + "-", string.Join('-', Enumerable.Repeat("dovha-nazva", 20)));
    }

    [Fact]
    public void WithSuffix_AppendsNumberFromTwoAndKeepsLength()
    {
        Assert.Equal("zamky", SlugText.WithSuffix("zamky", 1));
        Assert.Equal("zamky-2", SlugText.WithSuffix("zamky", 2));
        var longSlug = string.Join('-', Enumerable.Repeat("abcdefghi", 8));
        Assert.True(SlugText.WithSuffix(longSlug, 12).Length <= SlugText.MaxLength);
        Assert.EndsWith("-12", SlugText.WithSuffix(longSlug, 12));
    }

    [Theory]
    [InlineData("zamky", true)]
    [InlineData("zamky-2", true)]
    [InlineData("Zamky", false)]
    [InlineData("zamky--2", false)]
    [InlineData("-zamky", false)]
    [InlineData("zamky_2", false)]
    [InlineData("", false)]
    public void IsValid_ChecksPatternAndLength(string slug, bool expected)
        => Assert.Equal(expected, SlugText.IsValid(slug));

    [Fact]
    public void IsValid_RejectsTooLong()
        => Assert.False(SlugText.IsValid(new string('a', SlugText.MaxLength + 1)));

    [Fact]
    public void Allocator_AddsCollisionSuffixAndNeverReusesHistory()
    {
        var allocator = new SlugAllocator(CatalogEntityType.Category, ["zamky", "zamky-2"]);

        Assert.Equal("zamky-3", allocator.Allocate("Замки"));
        Assert.Equal("zamky-4", allocator.Allocate("Замки"));
        Assert.Equal("ruchky", allocator.Allocate("Ручки"));
    }

    [Theory]
    [InlineData("zamki-900002", 900002, true)]
    [InlineData("cosmos-920000", 920000, true)]
    [InlineData("zamky", 900002, false)]
    [InlineData("zamky-2", 900002, false)]
    public void MigrateLegacySlugs_DetectsIdSuffix(string slug, int id, bool expected)
        => Assert.Equal(expected, MigrateLegacySlugsJob.IsLegacy(slug, id));
}
