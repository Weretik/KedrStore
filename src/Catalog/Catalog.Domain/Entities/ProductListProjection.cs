using Catalog.Domain.Errors;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities;

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
            retailPrice);
    #endregion

    #region Validation & Setters
    private static string BuildSearchText(ProductId productId, string name)
        => $"{productId.Value} {name}".Trim();

    private void SetProductId(ProductId productId)
    {
        if (productId.Value <= 0) throw new DomainException(ProductErrors.IdRequired());

        ProductId = productId;
    }

    private void SetCategoryId(ProductCategoryId categoryId)
    {
        if (categoryId.Value <= 0) throw new DomainException(ProductErrors.CategoryIdRequired());

        CategoryId = categoryId;
    }

    private void SetCategorySlug(string categorySlug)
    {
        if (string.IsNullOrWhiteSpace(categorySlug))
        {
            throw new DomainException(CategoryErrors.SlugIsRequired());
        }

        CategorySlug = categorySlug.Trim();
    }

    private void SetNameUk(string nameUk)
    {
        NameUk = NormalizeName(nameUk);
    }

    private void SetNameRu(string nameRu)
    {
        NameRu = NormalizeName(nameRu);
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(ProductErrors.NameRequired());
        }

        var trimmed = name.Trim();
        if (trimmed.Length is < 1 or > 300)
        {
            throw new DomainException(ProductErrors.NameLengthInvalid(trimmed.Length));
        }

        return trimmed;
    }

    private void SetProductSlug(string productSlug)
    {
        if (string.IsNullOrWhiteSpace(productSlug))
        {
            throw new DomainException(ProductErrors.NameRequired());
        }

        ProductSlug = productSlug.Trim();
    }

    private void SetPhoto(string photo)
    {
        if (string.IsNullOrWhiteSpace(photo))
        {
            throw new DomainException(ProductErrors.PhotoRequired());
        }

        Photo = photo.Trim();
    }
    #endregion
}
