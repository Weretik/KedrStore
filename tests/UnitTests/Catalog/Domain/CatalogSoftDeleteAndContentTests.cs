using UnitTests.BuildingBlocks.Results;
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

public sealed class CatalogSoftDeleteAndContentTests
{
    private static readonly DateTimeOffset Created = new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Later = Created.AddDays(1);

    [Fact]
    public void Product_UpdateFromOneC_ReportsNoChangeAndKeepsUpdatedAt()
    {
        var product = NewProduct();

        var changed = product.UpdateFromOneC("Замок", ProductCategoryId.From(10), 1, true, false, false, Later).Value;

        Assert.False(changed);
        Assert.Null(product.UpdatedAt);
        Assert.Equal("zamok", product.ProductSlug);
    }

    [Fact]
    public void Product_UpdateFromOneC_NeverChangesSlug()
    {
        var product = NewProduct();

        var changed = product.UpdateFromOneC("Новий замок", ProductCategoryId.From(10), 1, true, true, false, Later).Value;

        Assert.True(changed);
        Assert.Equal(Later, product.UpdatedAt);
        Assert.Equal("zamok", product.ProductSlug);
        Assert.True(product.IsNew);
    }

    [Fact]
    public void Product_SoftDeleteAndRestore()
    {
        var product = NewProduct();

        Assert.True(product.MarkDeleted(Later));
        Assert.False(product.MarkDeleted(Later));
        Assert.True(product.IsDeleted);
        Assert.Equal(Later, product.DeletedAt);

        Assert.True(product.Restore(Later.AddDays(1)));
        Assert.False(product.IsDeleted);
        Assert.Null(product.DeletedAt);
        Assert.Equal("zamok", product.ProductSlug);
    }

    [Fact]
    public void Category_SoftDeleteRestoreAndChangeDetection()
    {
        var category = ProductCategory.Create(ProductCategoryId.From(10), "R", "Замки", "zamky", CategoryPath.From("n1.n10"),
            ProductCategoryId.From(1)).Value;

        Assert.False(category.UpdateFromOneC("Замки", CategoryPath.From("n1.n10"), ProductCategoryId.From(1), Later).Value);
        Assert.Null(category.UpdatedAt);
        Assert.True(category.MarkDeleted(Later));
        Assert.True(category.Restore(Later));
        Assert.True(category.UpdateFromOneC("Замки врізні", CategoryPath.From("n1.n10"), ProductCategoryId.From(1), Later).Value);
        Assert.Equal("zamky", category.Slug);
    }

    [Fact]
    public void CategoryContent_ReplacesFaqInOrderAndTrimsEmptyToNull()
    {
        var content = CategoryContent.Create(ProductCategoryId.From(10), "UK");

        content.Replace(" Замки ", "", null, "<p>Текст</p>", [("Питання 1", "Відповідь 1"), ("Питання 2", "Відповідь 2")],
            "admin-id", Later);

        Assert.Equal("uk", content.Language);
        Assert.Equal("Замки", content.SeoTitle);
        Assert.Null(content.MetaDescription);
        Assert.Equal([0, 1], content.Faq.Select(item => item.SortOrder));
        Assert.Equal("admin-id", content.UpdatedBy);
        Assert.False(content.IsEmpty);

        content.Replace(null, null, null, null, [], "admin-id", Later);
        Assert.True(content.IsEmpty);
    }

    [Fact]
    public void CategoryContent_EnforcesLimits()
    {
        var content = CategoryContent.Create(ProductCategoryId.From(10), "ru");

        DomainAssert.Fails(content.Replace(new string('a', 71), null, null, null, [], "admin", Later));
        DomainAssert.Fails(content.Replace(null, null, null, null,
            Enumerable.Range(0, 21).Select(index => ($"q{index}", "a")).ToList(), "admin", Later));
        DomainAssert.Fails(content.Replace(null, null, null, null, [("q", " ")], "admin", Later));
        Assert.Throws<ArgumentOutOfRangeException>(() => CategoryContent.Create(ProductCategoryId.From(10), "en"));
    }

    private static Product NewProduct()
        => Product.Create(ProductId.From(1), "R", "Замок", "zamok", ProductCategoryId.From(10),
            Created, 0, 1, false, false, true).Value;
}
