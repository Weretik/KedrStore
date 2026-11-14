using Catalog.Domain.Categories;
using Catalog.Domain.Products.Errors;

namespace Catalog.Domain.Products;

/// <summary>A 1C product. Its photo and scheme links live in <c>Product.Media.cs</c>.</summary>
public partial class Product : BaseAuditableEntity<ProductId>, IAggregateRoot
{
    public const decimal MaximumStock = 10_000m;
    public const int NameMaxLength = 300;

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
    #endregion

    #region Factories
    /// <summary>A new product starts without photos and scheme; they are linked from the media library (R-32).</summary>
    public static DomainResult<Product> Create(ProductId id, string productTypeIdOneC, string name, string productSlug,
        ProductCategoryId categoryId, DateTimeOffset createdDate, decimal stock, int qtyInPack, bool isNew, bool isSale,
        bool exportToSite = false)
    {
        var checks = new DomainChecks();
        if (id.Value <= 0)
            checks.Check(ProductErrors.IdRequired());
        var checkedName = checks.Take(CheckName(name));
        var slug = checks.Take(CheckSlug(productSlug));
        checks.Check(CheckCategoryId(categoryId));
        checks.Check(CheckStock(stock));
        checks.Check(CheckQuantityInPack(qtyInPack));
        if (checks.Error is { } error)
            return error;

        var product = new Product
        {
            Id = id,
            Name = checkedName,
            ProductSlug = slug,
            CategoryId = categoryId,
            Stock = stock,
            QuantityInPack = qtyInPack,
            IsNew = isNew,
            IsSale = isSale,
            ExportToSite = exportToSite,
            ProductTypeIdOneC = productTypeIdOneC,
        };
        product.MarkAsCreated(createdDate);
        return product;
    }

    /// <summary>
    /// Applies 1C-owned fields. The slug and the media links are not 1C-owned and never change here (R-32).
    /// Returns whether anything changed; <see cref="BaseAuditableEntity{TId}.UpdatedAt"/> moves only then.
    /// </summary>
    public DomainResult<bool> UpdateFromOneC(string name, ProductCategoryId categoryId,
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

        var checks = new DomainChecks();
        var checkedName = checks.Take(CheckName(name!));
        checks.Check(CheckCategoryId(categoryId));
        checks.Check(CheckQuantityInPack(qtyInPack));
        if (checks.Error is { } error)
            return error;

        Name = checkedName;
        CategoryId = categoryId;
        QuantityInPack = qtyInPack;
        ExportToSite = exportToSite;
        IsNew = isNew;
        IsSale = isSale;
        MarkAsUpdated(now);
        return true;
    }
    #endregion

    #region Lifecycle
    public DomainResult ChangeSlug(string slug, DateTimeOffset now)
    {
        var checkedSlug = CheckSlug(slug);
        if (checkedSlug.IsFailure)
            return checkedSlug;
        ProductSlug = checkedSlug.Value;
        MarkAsUpdated(now);
        return DomainResult.Success();
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

    #region Validation

    private static DomainResult CheckCategoryId(ProductCategoryId categoryId)
        => categoryId.Value <= 0 ? ProductErrors.CategoryIdRequired() : DomainResult.Success();

    private static DomainResult<string> CheckName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ProductErrors.NameRequired();

        var trimmed = name.Trim();
        return trimmed.Length > NameMaxLength ? ProductErrors.NameTooLong(trimmed.Length) : trimmed;
    }

    private static DomainResult<string> CheckSlug(string slug)
        => string.IsNullOrWhiteSpace(slug) ? ProductErrors.SlugRequired() : slug.Trim();

    private static DomainResult CheckStock(decimal stock)
        => stock < 0m || stock > MaximumStock ? ProductErrors.StockOutOfRange(stock) : DomainResult.Success();

    private static DomainResult CheckQuantityInPack(int qtyInPack)
        => qtyInPack < 0 ? ProductErrors.QuantityInPackNegative() : DomainResult.Success();
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
    public DomainResult UpdateStock(decimal stock)
    {
        var checkedStock = CheckStock(stock);
        if (checkedStock.IsFailure)
            return checkedStock;
        Stock = stock;
        return DomainResult.Success();
    }
    #endregion
}
