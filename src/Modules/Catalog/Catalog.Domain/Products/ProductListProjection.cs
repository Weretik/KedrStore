using Catalog.Domain.Categories;

namespace Catalog.Domain.Products;

public sealed class ProductListProjection
{
    #region Properties
    public ProductId ProductId { get; private set; }
    public string NameUk { get; private set; } = null!;
    public string NameRu { get; private set; } = null!;
    public string ProductSlug { get; private set; } = null!;
    public string Photo { get; private set; } = null!;
    public ProductCategoryId CategoryId { get; private set; }
    public string CategorySlug { get; private set; } = null!;
    public bool InStock { get; private set; }
    public bool IsSale { get; private set; }
    public bool IsNew { get; private set; }
    public bool ExportToSite { get; private set; }
    public decimal? RetailPrice { get; private set; }
    public string SearchTextUk { get; private set; } = null!;
    public string SearchTextRu { get; private set; } = null!;
    #endregion

    #region Constructors
    private ProductListProjection() { }

    private ProductListProjection(
        ProductId productId,
        string nameUk,
        string nameRu,
        string productSlug,
        string photo,
        ProductCategoryId categoryId,
        string categorySlug,
        bool inStock,
        bool isSale,
        bool isNew,
        bool exportToSite,
        decimal? retailPrice)
    {
        SetProductId(productId);
        SetNameUk(nameUk);
        SetNameRu(nameRu);
        SetProductSlug(productSlug);
        SetPhoto(photo);
        SetCategoryId(categoryId);
        SetCategorySlug(categorySlug);

        InStock = inStock;
        IsSale = isSale;
        IsNew = isNew;
        ExportToSite = exportToSite;
        RetailPrice = retailPrice;
        SearchTextUk = BuildSearchText(productId, NameUk);
        SearchTextRu = BuildSearchText(productId, NameRu);
    }
    #endregion

    #region Factories
    public static ProductListProjection Create(
        ProductId productId,
        string nameUk,
        string nameRu,
        string productSlug,
        string photo,
        ProductCategoryId categoryId,
        string categorySlug,
        bool inStock,
        bool isSale,
        bool isNew,
        bool exportToSite,
        decimal? retailPrice)
        => new(
            productId,
            nameUk,
            nameRu,
            productSlug,
            photo,
            categoryId,
            categorySlug,
            inStock,
            isSale,
            isNew,
            exportToSite,
            retailPrice);
    #endregion

    #region Guards & Setters
    private static string BuildSearchText(ProductId productId, string name)
        => $"{productId.Value} {name}".Trim();

    // The projection is rebuilt from stored products that already passed the domain rules, so a bad value here is a
    // bug in the rebuilder, not user input.
    private void SetProductId(ProductId productId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(productId.Value, nameof(productId));
        ProductId = productId;
    }

    private void SetCategoryId(ProductCategoryId categoryId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(categoryId.Value, nameof(categoryId));
        CategoryId = categoryId;
    }

    private void SetCategorySlug(string categorySlug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(categorySlug);
        CategorySlug = categorySlug.Trim();
    }

    private void SetNameUk(string nameUk) => NameUk = NormalizeName(nameUk);

    private void SetNameRu(string nameRu) => NameRu = NormalizeName(nameRu);

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var trimmed = name.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, Product.NameMaxLength, nameof(name));
        return trimmed;
    }

    private void SetProductSlug(string productSlug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productSlug);
        ProductSlug = productSlug.Trim();
    }

    /// <summary>Empty when the product has no linked photo (R-30).</summary>
    private void SetPhoto(string? photo) => Photo = photo?.Trim() ?? string.Empty;
    #endregion
}
