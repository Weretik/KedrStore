namespace Domain.Catalog.Entities;

public class ProductCategory : BaseCategory<ProductCategoryId, ProductCategory>, IAggregateRoot
{
    #region Constructors
    private ProductCategory() { }
    private ProductCategory(ProductCategoryId id, string name, LTree path)
    {
        SetCategoryId(id);
        SetName(name);
        SetPath(path);
    }
    #endregion

    #region Factories
    public static ProductCategory Create(ProductCategoryId id, string name, LTree path)
        => new(id, name, path);
    public static ProductCategory CreateRoot(ProductCategoryId id, string name)
        => new(id, name, new LTree($"n{id}"));
    public static ProductCategory CreateChild(ProductCategoryId id, string name, LTree parentPath)
        => new(id, name, new LTree($"{parentPath}.n{id}"));
    public static ProductCategory CreateChild(ProductCategoryId id, string name, ProductCategory parent)
        => new(id, name, new LTree($"{parent.Path}.n{id}"));
    #endregion

}
