using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace UnitTests;

public sealed class ProductCategoryPresentationMetadataTests
{
    [Fact]
    public void UpdatePresentationMetadata_StoresShortNamesWithoutChangingOriginalName()
    {
        var category = CreateCategory("Повна назва з 1С", "n5513.n900001");

        category.UpdatePresentationMetadata("  Завіси  ", "  Петли  ", 3, 1);

        Assert.Equal("Повна назва з 1С", category.Name);
        Assert.Equal("Завіси", category.ShortNameUk);
        Assert.Equal("Петли", category.ShortNameRu);
        Assert.Equal(3, category.SortOrder);
        Assert.Equal(1, category.Level);
    }

    [Theory]
    [InlineData("", "Петли", "Catalog.Category.ShortNameUk.Required")]
    [InlineData("Завіси", "", "Catalog.Category.ShortNameRu.Required")]
    public void UpdatePresentationMetadata_RejectsBlankShortNames(
        string shortNameUk,
        string shortNameRu,
        string expectedCode)
    {
        var category = CreateCategory();

        var exception = Assert.Throws<DomainException>(
            () => category.UpdatePresentationMetadata(shortNameUk, shortNameRu, 0, 0));

        Assert.Equal(expectedCode, exception.Error.Code);
    }

    [Fact]
    public void UpdatePresentationMetadata_RejectsShortNameLongerThanMaximum()
    {
        var category = CreateCategory();

        var exception = Assert.Throws<DomainException>(
            () => category.UpdatePresentationMetadata(new string('a', ProductCategory.ShortNameMaxLength + 1), "Петли", 0, 0));

        Assert.Equal("Catalog.Category.ShortNameUk.LengthInvalid", exception.Error.Code);
    }

    [Theory]
    [InlineData(-1, 0, "Catalog.Category.SortOrder.Negative")]
    [InlineData(0, -1, "Catalog.Category.Level.Negative")]
    public void UpdatePresentationMetadata_RejectsNegativeOrderingValues(
        int sortOrder,
        int level,
        string expectedCode)
    {
        var category = CreateCategory();

        var exception = Assert.Throws<DomainException>(
            () => category.UpdatePresentationMetadata("Завіси", "Петли", sortOrder, level));

        Assert.Equal(expectedCode, exception.Error.Code);
    }

    [Fact]
    public void UpdatePresentationMetadata_RejectsLevelThatDoesNotMatchPath()
    {
        var category = CreateCategory(path: "n5513.n900001");

        var exception = Assert.Throws<DomainException>(
            () => category.UpdatePresentationMetadata("Завіси", "Петли", 0, 0));

        Assert.Equal("Catalog.Category.Level.PathMismatch", exception.Error.Code);
    }

    [Fact]
    public void Create_AppliesDeterministicPresentationFallbacks()
    {
        var category = CreateCategory("Категорія з 1С", "n5513.n900001");

        Assert.Equal("Категорія з 1С", category.ShortNameUk);
        Assert.Equal("Категорія з 1С", category.ShortNameRu);
        Assert.Equal(int.MaxValue, category.SortOrder);
        Assert.Equal(1, category.Level);
    }

    private static ProductCategory CreateCategory(string name = "Категорія", string path = "n5513")
        => ProductCategory.Create(
            ProductCategoryId.From(5513),
            "000005513",
            name,
            "furnitura-5513",
            CategoryPath.From(path));
}
