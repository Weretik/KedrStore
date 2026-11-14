using Catalog.Domain.Categories;

namespace Catalog.Domain.Products;

/// <summary>A 1C product. Its photo and scheme links live in <c>Product.Media.cs</c>.</summary>
public partial class Product : BaseAuditableEntity<ProductId>, IAggregateRoot
{
    public const decimal MaximumStock = 10_000m;

    #region Properties
    public string ProductTypeIdOneC { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string ProductSlug { get; private set; } = null!;
    public ProductCategoryId CategoryId { get; private set; }
    /// <summary>Cache of the first photo URL; empty without photos. The links are the source (R-30).</summary>
    public string Photo { get; private set; } = string.Empty;
    /// <summary>Cache of the scheme URL; empty without a scheme.</summary>
    public string Sсheme { get; private set; } = string.Empty;
    public decimal Stock { get; private set; }
    public bool IsSale { get; private set; }
    public bool IsNew { get; private set; }
    public bool ExportToSite { get; private set; }
    public int QuantityInPack { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    #endregion

    #region Constructors
    private Product() { }
    private Product(ProductId id, string productTypeIdOneC, string name, string productSlug, ProductCategoryId categoryId,
        DateTimeOffset createdDate, decimal stock, int qtyInPack, bool isNew, bool isSale)
    {
        SetProductId(id);
        SetName(name);
        SetProductSlug(productSlug);
        SetCategoryId(categoryId);
        SetStock(stock);
        MarkAsCreated(createdDate);
        SetQuantityInPack(qtyInPack);
        if(isNew) MarkAsNew();
        if(isSale) MarkAsSale();

        ProductTypeIdOneC = productTypeIdOneC;
    }
    #endregion

    #region Factories
    /// <summary>A new product starts without photos and scheme; they are linked from the media library (R-32).</summary>
    public static Product Create(ProductId id, string productTypeIdOneC, string name, string productSlug, ProductCategoryId categoryId,
        DateTimeOffset createdDate, decimal stock, int qtyInPack, bool isNew, bool isSale)
        => new(id, productTypeIdOneC, name, productSlug, categoryId, createdDate, stock, qtyInPack, isNew, isSale);

    public static Product Create(ProductId id, string productTypeIdOneC, string name, string productSlug, ProductCategoryId categoryId,
        DateTimeOffset createdDate, decimal stock, int qtyInPack, bool isNew, bool isSale,
        bool exportToSite)
    {
        var product = Create(
            id,
            productTypeIdOneC,
            name,
            productSlug,
            categoryId,
            createdDate,
            stock,
            qtyInPack,
            isNew,
            isSale);

        product.ExportToSite = exportToSite;
        return product;
    }

    /// <summary>
    /// Applies 1C-owned fields. The slug and the media links are not 1C-owned and never change here (R-32).
    /// Returns whether anything changed; <see cref="BaseAuditableEntity{TId}.UpdatedAt"/> moves only then.
    /// </summary>
    public bool UpdateFromOneC(string name, ProductCategoryId categoryId,
        int qtyInPack, bool exportToSite, bool isNew, bool isSale, DateTimeOffset now)
    {
        var changed = !string.Equals(Name, name?.Trim(), StringComparison.Ordinal) ||
                      CategoryId != categoryId ||
                      QuantityInPack != qtyInPack ||
                      ExportToSite != exportToSite ||
                      IsNew != isNew ||
                      IsSale != isSale;

        if (!changed)
            return false;

        SetName(name!);
        SetCategoryId(categoryId);
        SetQuantityInPack(qtyInPack);
        ExportToSite = exportToSite;
        IsNew = isNew;
        IsSale = isSale;
        MarkAsUpdated(now);
        return true;
    }
    #endregion

    #region Lifecycle
    public void ChangeSlug(string slug, DateTimeOffset now)
    {
        SetProductSlug(slug);
        MarkAsUpdated(now);
    }

    public bool MarkDeleted(DateTimeOffset now)
    {
        if (IsDeleted) return false;
        MarkAsDeleted(now);
        DeletedAt = now;
        return true;
    }

    public bool Restore(DateTimeOffset now)
    {
        if (!IsDeleted) return false;
        IsDeleted = false;
        DeletedAt = null;
        MarkAsUpdated(now);
        return true;
    }
    #endregion

    #region Validation & Setters

    private void SetProductId(ProductId id)
    {
        if (id.Value <= 0) throw new DomainException(ProductErrors.IdRequired());
        Id = id;
    }
    private void SetCategoryId(ProductCategoryId categoryId)
    {
        if (categoryId.Value <= 0) throw new DomainException(ProductErrors.CategoryIdRequired());
        CategoryId = categoryId;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ProductErrors.NameRequired());

        var trimmed = name.Trim();

        if (trimmed.Length is < 1 or > 300)
            throw new DomainException(ProductErrors.NameLengthInvalid(trimmed.Length));

        Name = trimmed;
    }

    private void SetProductSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) throw new DomainException(ProductErrors.NameRequired());
        ProductSlug = slug.Trim();
    }

    private void SetStock(decimal stock)
    {
        if (stock < 0m || stock > MaximumStock)
            throw new DomainException(ProductErrors.StockOutOfRange(stock));

        Stock = stock;
    }

    private void SetQuantityInPack(int qtyInPack)
    {
        if (qtyInPack < 0)
            throw new DomainException(ProductErrors.QuantityInPackNegative(qtyInPack));

        QuantityInPack = qtyInPack;
    }
    #endregion

    #region Markers
    public void MarkAsNew()
    {
        IsNew = true;
    }
    public void MarkAsSale()
    {
        IsSale = true;
    }
    public void RemoveSale()
    {
        IsSale = false;
    }
    public void RemoveNew()
    {
        IsNew = false;
    }
    #endregion

    #region Update API
    public void UpdateStock(decimal stock) => SetStock(stock);
    #endregion
}
