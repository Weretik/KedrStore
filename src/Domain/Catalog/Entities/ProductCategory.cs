using Domain.Catalog.ValueObjects;
using Domain.Common.ValueObject;

namespace Domain.Catalog.Entities;

public class ProductCategory : BaseCategory<ProductCategoryId, ProductCategory>, IAggregateRoot
{
    #region Constructors
    private ProductCategory() { }
    private ProductCategory(ProductCategoryId id, string name, CategoryPath path)
    {
        SetCategoryId(id);
        SetName(name);
        SetPath(path);
    }
    #endregion

    #region Factories
    public static ProductCategory Create(ProductCategoryId id, string name, CategoryPath path)
        => new(id, name, path);
    public static ProductCategory CreateRoot(ProductCategoryId id, string name)
        => new(id, name, CategoryPath.Root(id.ToString()));
    public static ProductCategory CreateChild(ProductCategoryId id, string name, ProductCategory parent)
        => new(id, name, parent.Path.Append(id.ToString()));
    #endregion

}
