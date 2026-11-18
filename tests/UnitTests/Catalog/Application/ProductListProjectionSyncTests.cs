using BuildingBlocks.Domain.Identifiers;
using Catalog.Domain.Categories;
using Catalog.Application.Features.Products.Shared;
using Catalog.Domain.Products;

namespace UnitTests.Catalog.Application;

/// <summary>A rebuild writes only the list rows whose values changed (database rules 2.4).</summary>
public sealed class ProductListProjectionSyncTests
{
    [Fact]
    public void SyncWith_SameValues_ReportsNoChange()
    {
        var row = Row("Ручка", price: 100m);

        Assert.False(row.SyncWith(Row("Ручка", price: 100m)));
    }

    [Fact]
    public void SyncWith_ChangedValues_CopiesEveryFieldAndSearchText()
    {
        var row = Row("Ручка", price: 100m);

        var changed = row.SyncWith(ProductListProjection.Create(ProductId.Create(7), "Ручка нова", "Ручка новая",
            "ruchka-nova", "photo.jpg", ProductCategoryId.Create(3), "furnitura", inStock: false, isSale: true,
            isNew: true, exportToSite: false, retailPrice: 120m));

        Assert.True(changed);
        Assert.Equal("Ручка нова", row.NameUk);
        Assert.Equal("Ручка новая", row.NameRu);
        Assert.Equal("ruchka-nova", row.ProductSlug);
        Assert.Equal("photo.jpg", row.Photo);
        Assert.Equal(ProductCategoryId.Create(3), row.CategoryId);
        Assert.Equal("furnitura", row.CategorySlug);
        Assert.False(row.InStock);
        Assert.True(row.IsSale);
        Assert.True(row.IsNew);
        Assert.False(row.ExportToSite);
        Assert.Equal(120m, row.RetailPrice);
        Assert.Equal("7 Ручка нова", row.SearchTextUk);
        Assert.Equal("7 Ручка новая", row.SearchTextRu);
    }

    [Fact]
    public void SyncWith_AnotherProduct_Throws()
    {
        var row = Row("Ручка", price: 100m);
        var other = ProductListProjection.Create(ProductId.Create(8), "Ручка", "Ручка", "ruchka", string.Empty,
            ProductCategoryId.Create(2), "dveri", true, false, false, true, 100m);

        Assert.Throws<ArgumentException>(() => row.SyncWith(other));
    }

    private static ProductListProjection Row(string name, decimal? price)
        => ProductListProjection.Create(ProductId.Create(7), name, name, "ruchka", string.Empty,
            ProductCategoryId.Create(2), "dveri", inStock: true, isSale: false, isNew: false, exportToSite: true,
            retailPrice: price);
}
