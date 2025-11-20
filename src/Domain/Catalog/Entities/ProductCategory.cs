using Domain.Catalog.Enumerations;
using Domain.Catalog.ValueObjects;
using Domain.Common.ValueObject;

namespace Domain.Catalog.Entities;

public class ProductCategory : BaseCategory<ProductCategoryId, ProductCategory>, IAggregateRoot
{
    #region Properties
    public ProductType ProductType { get; private set; }
    #endregion

    #region Constructors
    private ProductCategory() { }
    private ProductCategory(ProductCategoryId id, string name, CategoryPath path, ProductType productType)
    {
        SetCategoryId(id);
        SetName(name);
        SetPath(path);
        SetProductType(productType);
    }
    #endregion

    #region Factories
    public static ProductCategory Create(ProductCategoryId id, string name, CategoryPath path, ProductType productType)
        => new(id, name, path, productType);
    public static ProductCategory CreateRoot(ProductCategoryId id, string name, ProductType productType)
        => new(id, name, CategoryPath.Root(id.ToString()), productType);
    public static ProductCategory CreateChild(ProductCategoryId id, string name, ProductCategory parent, ProductType productType)
        => new(id, name, parent.Path.Append(id.ToString()), productType);
    #endregion

    #region Validation & Setters
    private void SetProductType(ProductType productType) => ProductType = Guard.Against.Default(productType, nameof(productType));
    #endregion
}
