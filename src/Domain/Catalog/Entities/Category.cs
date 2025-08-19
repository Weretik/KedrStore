namespace Domain.Catalog.Entities;

public class Category : BaseEntity<CategoryId>, IAggregateRoot
{
    #region Properties

    public string Name { get; private set; } = null!;
    public CategoryId? ParentCategoryId { get; private set; }
    private readonly List<Category> _children = new();
    public IReadOnlyCollection<Category> Children => _children.AsReadOnly();

    #endregion

    #region Constructors
    private Category() { }

    private Category(CategoryId id, string name, DateTime createdDate, CategoryId? parentCategoryId = null)
    {
        SetId(id);
        SetName(name);
        SetParentCategoryId(parentCategoryId);
        MarkAsCreated(createdDate);
    }

    public static Category Create(CategoryId id, string name, DateTime createdDate, CategoryId? parentCategoryId = null)
    {
        return new Category(id, name, createdDate, parentCategoryId);
    }

    #endregion

    #region Validation & Setters

    private void SetId(CategoryId id)
    {
        Id = Guard.Against.Null(id, nameof(id));
    }
    private void SetName(string name)
    {
        Name = Guard.Against.NullOrWhiteSpace(name, nameof(name)).Trim();;
    }
    private void SetParentCategoryId(CategoryId? parentCategoryId)
    {
        ParentCategoryId = parentCategoryId;
    }

    private bool HasDescendant(CategoryId candidateId)
    {
        var stack = new Stack<Category>(_children);
        while (stack.Count > 0)
        {
            var node = stack.Pop();

            if (node.Id == candidateId) return true;

            if (node._children.Count > 0)
            {
                foreach (var child in node._children)
                {
                    stack.Push(child);
                }
            }
        }
        return false;
    }
    #endregion

    #region Update & Change Methods

    public void Update(string name, DateTime updatedDate, CategoryId? parentCategoryId = null)
    {
        ChangeName(name, updatedDate);
        ChangeParent(parentCategoryId, updatedDate);
    }

    public void ChangeName(string name, DateTime updatedDate)
    {
        SetName(name);
        MarkAsUpdated(updatedDate);
    }

    public void ChangeParent(CategoryId? parentCategoryId, DateTime updatedDate)
    {
        if (parentCategoryId is not null)
        {
            Guard.Against.InvalidInput(parentCategoryId, nameof(parentCategoryId),
                parentId => parentId != Id,
                "You cannot assign yourself as the parent category.");

            Guard.Against.InvalidInput(parentCategoryId, nameof(parentCategoryId),
                parentId => !HasDescendant(parentId),
                "You cannot assign a child category to a parent (cycle).");
        }

        SetParentCategoryId(parentCategoryId);
        MarkAsUpdated(updatedDate);

    }

    #endregion

    #region Manipulations with child categories
    public void AddChild(Category child, DateTime updatedDate)
    {
        child = Guard.Against.Null(child, nameof(child));

        Guard.Against.InvalidInput(child, nameof(child),
            c => c.Id != Id,
            "You cannot add a category to itself as a child category.");

        Guard.Against.InvalidInput(child.Id, nameof(child.Id),
            id => _children.All(c => c.Id != id),
            "Child already added.");

        Guard.Against.InvalidInput(child, nameof(child),
            c => c.ParentCategoryId is null || c.ParentCategoryId == Id,
            "Child belongs to another parent. Detach or move it first.");

        Guard.Against.InvalidInput(child, nameof(child),
            c => !c.HasDescendant(Id),
            "You cannot add an ancestor as a child (cycle).");

        _children.Add(child);
        child.ParentCategoryId = Id;
        MarkAsUpdated(updatedDate);

    }

    public void RemoveChild(Category child, DateTime updatedDate)
    {
        child = Guard.Against.Null(child, nameof(child));

        var node = _children.FirstOrDefault(c => c.Id == child.Id);

        if (node is null)
            return;

        _children.Remove(node);

        if (node.ParentCategoryId == Id)
            node.ParentCategoryId = null;

        MarkAsUpdated(updatedDate);
    }
    #endregion
}
