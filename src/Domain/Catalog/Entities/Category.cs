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

        AddCreatedEvent();
    }

    public static Category Create(CategoryId id, string name, DateTime createdDate, CategoryId? parentCategoryId = null)
    {
        return new Category(id, name, createdDate, parentCategoryId);
    }

    #endregion

    #region Validation & Setters

    private void SetId(CategoryId id)
    {
        RuleChecker.Check(new IdMustNotBeNullRule(id));
        Id = id;
    }
    private void SetName(string name)
    {
        RuleChecker.Check(new NameMustNotBeEmptyRule(name));
        Name = name;
    }
    private void SetParentCategoryId(CategoryId? parentCategoryId)
    {
        ParentCategoryId = parentCategoryId;
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
        if(parentCategoryId != null)
        {
            RuleChecker.Check(new CannotAssignSelfOrDescendantAsParentRule(this, parentCategoryId));
        }
        SetParentCategoryId(parentCategoryId);
        MarkAsUpdated(updatedDate);
    }

    public bool IsDescendantOf(CategoryId targetAncestorId)
    {
        var current = this;
        while (current.ParentCategoryId != null)
        {
            if (current.ParentCategoryId == targetAncestorId)
                return true;

            break;
        }

        return false;
    }
    #endregion

    #region Manipulations with child categories
    public void AddChild(Category child, DateTime updatedDate)
    {
        RuleChecker.Check(new ChildCategoryMustNotBeNullRule(child));
        RuleChecker.Check(new CannotAddSelfOrAncestorAsChildRule(this, child));

        if (!_children.Contains(child))
        {
            _children.Add(child);
            child.ParentCategoryId = Id;
            MarkAsUpdated(updatedDate);
        }
    }

    public void RemoveChild(Category child, DateTime updatedDate)
    {
        RuleChecker.Check(new ChildCategoryMustNotBeNullRule(child));

        if (_children.Remove(child))
        {
            child.ParentCategoryId = null;
            MarkAsUpdated(updatedDate);
        }
    }
    #endregion

    #region Domain Events

    private void AddCreatedEvent() => AddDomainEvent(new CategoryCreatedEvent(Id));

    #endregion
}
