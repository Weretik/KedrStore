using Catalog.Domain.Errors;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities;

public class Product : BaseAuditableEntity<ProductId>, IAggregateRoot
{
    public const decimal MaximumStock = 10_000m;

    #region Properties
    public string ProductTypeIdOneC { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string ProductSlug { get; private set; } = null!;
    public ProductCategoryId CategoryId { get; private set; }
    public string Photo { get; private set; } = null!;
    public string Sсheme { get; private set; } = null!;
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
        string photo, DateTimeOffset createdDate, decimal stock, string sсheme, int qtyInPack, bool isNew, bool isSale)
    {
        SetProductId(id);
        SetName(name);
        SetProductSlug(productSlug);
        SetCategoryId(categoryId);
        SetPhoto(photo);
        SetSсheme(sсheme);
        SetStock(stock);
        MarkAsCreated(createdDate);
        SetQuantityInPack(qtyInPack);
        if(isNew) MarkAsNew();
        if(isSale) MarkAsSale();

        ProductTypeIdOneC = productTypeIdOneC;
    }
    #endregion

    #region Factories
    public static Product Create(ProductId id, string productTypeIdOneC, string name, string productSlug, ProductCategoryId categoryId,
        string photo, string scheme, DateTimeOffset createdDate, decimal stock, int qtyInPack, bool isNew, bool isSale)
        => new(id, productTypeIdOneC, name, productSlug, categoryId, photo, createdDate, stock, scheme, qtyInPack, isNew, isSale);

    public static Product Create(ProductId id, string productTypeIdOneC, string name, string productSlug, ProductCategoryId categoryId,
        string photo, string scheme, DateTimeOffset createdDate, decimal stock, int qtyInPack, bool isNew, bool isSale,
        bool exportToSite)
    {
        var product = Create(
            id,
            productTypeIdOneC,
            name,
            productSlug,
            categoryId,
            photo,
            scheme,
            createdDate,
            stock,
            qtyInPack,
            isNew,
            isSale);

        product.ExportToSite = exportToSite;
        return product;
    }

    /// <summary>
    /// Applies 1C-owned fields. The slug is not 1C-owned and never changes here.
    /// Returns whether anything changed; <see cref="BaseAuditableEntity{TId}.UpdatedAt"/> moves only then.
    /// </summary>
    public bool UpdateFromOneC(string name, ProductCategoryId categoryId, string photo, string scheme,
        int qtyInPack, bool exportToSite, bool isNew, bool isSale, DateTimeOffset now)
    {
        var changed = !string.Equals(Name, name?.Trim(), StringComparison.Ordinal) ||
                      CategoryId != categoryId ||
                      !string.Equals(Photo, photo?.Trim(), StringComparison.Ordinal) ||
                      !string.Equals(Sсheme, scheme?.Trim(), StringComparison.Ordinal) ||
                      QuantityInPack != qtyInPack ||
                      ExportToSite != exportToSite ||
                      IsNew != isNew ||
                      IsSale != isSale;

        if (!changed)
            return false;

        SetName(name!);
        SetCategoryId(categoryId);
        SetPhoto(photo!);
        SetSсheme(scheme!);
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

    private void SetPhoto(string photo)
    {
        if (string.IsNullOrWhiteSpace(photo))
            throw new DomainException(ProductErrors.PhotoRequired());

        Photo = photo.Trim();
    }

    private void SetSсheme(string scheme)
    {
        if (string.IsNullOrWhiteSpace(scheme))
            throw new DomainException(ProductErrors.SchemeInvalid());

        Sсheme = scheme.Trim();
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
