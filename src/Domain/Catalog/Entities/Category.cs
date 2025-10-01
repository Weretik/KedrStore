namespace Domain.Catalog.Entities;

public class Category : CategoryNode<CategoryId, Category>, IAggregateRoot
{
    private Category() { }
    private Category(CategoryId id, string name, LTree path, DateTime createdDate, CategoryId? parentId)
    {
        Id = Guard.Against.Default(id, nameof(id));
        SetName(name);
        SetParent(parentId);
        SetPath(path);
    }

    public static Category Create(CategoryId id, string name, LTree path, DateTime createdAt,
        CategoryId? parentId = null)
        => new(id, name, path, createdAt, parentId);

    public static Category CreateRoot(CategoryId id, string name, DateTime createdAt)
        => new(
            id: id,
            name: name,
            path: (LTree)$"root.n{id}",
            createdDate: createdAt,
            parentId: null);

    public static Category CreateChild(CategoryId id, string name, DateTime createdAt,
        Category parent)
        => new(
            id: id,
            name: name,
            path: parent.Path + (LTree)$".n{id}",
            createdDate: createdAt,
            parentId: parent.Id);
}
