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

/// <summary>R-30, R-33, R-34, R-35 on the product aggregate.</summary>
public sealed class ProductMediaTests
{
    private static readonly DateTimeOffset Created = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Later = Created.AddHours(1);
    private static readonly ProductMediaFile A = new(1, "https://cdn.test/a.jpg");
    private static readonly ProductMediaFile B = new(2, "https://cdn.test/b.jpg");
    private static readonly ProductMediaFile C = new(3, "https://cdn.test/c.jpg");

    [Fact]
    public void NewProduct_HasNoMedia_AndEmptyCache()
    {
        var product = NewProduct();

        Assert.Empty(product.MediaLinks);
        Assert.Equal(string.Empty, product.Photo);
        Assert.Equal(string.Empty, product.Sсheme);
    }

    [Fact]
    public void SetMedia_KeepsOrder_AndCachesFirstPhotoAndScheme()
    {
        var product = NewProduct();

        Assert.True(product.SetMedia([B, A], C, Later));

        Assert.Equal([2L, 1L], product.Photos.Select(link => link.MediaFileId));
        Assert.Equal(3, product.Scheme!.MediaFileId);
        Assert.Equal(B.Url, product.Photo);
        Assert.Equal(C.Url, product.Sсheme);
        Assert.Equal(Later, product.UpdatedAt);
        Assert.False(product.SetMedia([B, A], C, Later.AddHours(1)));
        Assert.Equal(Later, product.UpdatedAt);
    }

    [Fact]
    public void SetMedia_ReordersAndClears()
    {
        var product = NewProduct();
        product.SetMedia([B, A], C, Later);

        Assert.True(product.SetMedia([A], null, Later));

        Assert.Equal(A.Url, Assert.Single(product.MediaLinks).Url);
        Assert.Equal(string.Empty, product.Sсheme);
        Assert.True(product.SetMedia([], null, Later));
        Assert.Equal(string.Empty, product.Photo);
    }

    [Fact]
    public void SetMedia_RejectsDuplicatesAndMoreThanTwentyPhotos()
    {
        var product = NewProduct();

        Assert.Throws<DomainException>(() => product.SetMedia([A, A], null, Later));
        Assert.Throws<DomainException>(() => product.SetMedia([A], A, Later));
        var many = Enumerable.Range(1, Product.MaximumPhotos + 1)
            .Select(id => new ProductMediaFile(id, $"https://cdn.test/{id}.jpg")).ToArray();
        Assert.Throws<DomainException>(() => product.SetMedia(many, null, Later));
    }

    [Fact]
    public void LinkIfNone_LinksOnlyWhenTheKindIsMissing()
    {
        var product = NewProduct();

        Assert.True(product.LinkPhotoIfNone(A, Later));
        Assert.False(product.LinkPhotoIfNone(B, Later));
        Assert.False(product.LinkSchemeIfNone(A, Later));
        Assert.True(product.LinkSchemeIfNone(C, Later));

        Assert.Equal(A.Url, product.Photo);
        Assert.Equal(C.Url, product.Sсheme);
    }

    [Fact]
    public void ReplaceAndDelete_UpdateUrl_AndMoveNextPhotoFirst()
    {
        var product = NewProduct();
        product.SetMedia([A, B], null, Later);

        Assert.True(product.UpdateMediaUrl(A.Id, "https://cdn.test/a2.jpg", Later));
        Assert.Equal("https://cdn.test/a2.jpg", product.Photo);
        Assert.True(product.RemoveMediaFile(A.Id, Later));
        Assert.False(product.RemoveMediaFile(A.Id, Later));

        var first = Assert.Single(product.Photos);
        Assert.Equal((B.Id, 0), (first.MediaFileId, first.Position));
        Assert.Equal(B.Url, product.Photo);
    }

    [Fact]
    public void RefreshMedia_RereadsUrls_DropsMissingFiles_AndRewritesCache()
    {
        var product = NewProduct();
        product.SetMedia([A, B], C, Later);

        Assert.True(product.RefreshMedia(new Dictionary<long, string> { [B.Id] = "https://cdn.test/b2.jpg" }, Later));

        Assert.Equal("https://cdn.test/b2.jpg", Assert.Single(product.MediaLinks).Url);
        Assert.Equal("https://cdn.test/b2.jpg", product.Photo);
        Assert.Equal(string.Empty, product.Sсheme);
    }

    private static Product NewProduct()
        => Product.Create(ProductId.From(731), "R", "Петля", "petlia", ProductCategoryId.From(10),
            Created, 0, 1, false, false, true);
}
