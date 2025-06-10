using Domain.Abstractions;
using Domain.Catalog.ValueObjects;
using Domain.Common;
using Domain.Catalog.Events;

namespace Domain.Catalog.Entities;

public class Category : BaseEntity<CategoryId>, IAggregateRoot
{
    public string Name { get; private set; } = null!;
    public CategoryId? ParentCategoryId { get; private set; }
    private readonly List<Category> _children = new();
    public IReadOnlyCollection<Category> Children => _children.AsReadOnly();

    private Category() { }

    private Category(CategoryId id, string name, CategoryId? parentCategoryId = null)
    {
        Id = id;
        Name = name;
        ParentCategoryId = parentCategoryId;

        AddDomainEvent(new CategoryCreatedEvent(id));
    }

    private static void ValidateId(CategoryId id)
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));
        if (id.Value <= 0)
            throw new ArgumentException("CategoryId must be greater than zero", nameof(id));
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
    }

    private static void ValidateParentId(CategoryId? parentId)
    {
        if (parentId != null && parentId.Value <= 0)
            throw new ArgumentException("Parent CategoryId must be greater than zero", nameof(parentId));
    }

    public static Category Create(CategoryId id, string name, CategoryId? parentCategoryId = null)
    {
        ValidateId(id);
        ValidateName(name);
        ValidateParentId(parentCategoryId);

        return new Category(id, name, parentCategoryId);
    }

    public void Update(string name, CategoryId? parentCategoryId = null)
    {
        ValidateName(name);
        ValidateParentId(parentCategoryId);

        Name = name;
        ParentCategoryId = parentCategoryId;
        MarkAsUpdated();
        //AddDomainEvent(new CategoryUpdatedEvent(Id));
    }

    public void ChangeName(string name)
    {
        ValidateName(name);
        Name = name;
        MarkAsUpdated();
        //AddDomainEvent(new CategoryNameChangedEvent(Id, name));
    }

    public void ChangeParent(CategoryId? parentCategoryId)
    {
        ValidateParentId(parentCategoryId);

        // Проверка на циклическую зависимость
        if (parentCategoryId != null && (parentCategoryId == Id || IsDescendantOf(parentCategoryId)))
        {
            throw new InvalidOperationException("Cannot set parent: would create circular reference");
        }

        ParentCategoryId = parentCategoryId;
        MarkAsUpdated();
        //AddDomainEvent(new CategoryParentChangedEvent(Id, parentCategoryId));
    }

    public void AddChild(Category child)
    {
        if (child == null)
            throw new ArgumentNullException(nameof(child));

        if (child.Id == Id || IsDescendantOf(child.Id))
        {
            throw new InvalidOperationException("Cannot add child: would create circular reference");
        }

        if (!_children.Contains(child))
        {
            _children.Add(child);
            child.ParentCategoryId = Id;
            MarkAsUpdated();
            //AddDomainEvent(new CategoryChildAddedEvent(Id, child.Id));
        }
    }

    public void RemoveChild(Category child)
    {
        if (child == null)
            throw new ArgumentNullException(nameof(child));

        if (_children.Remove(child))
        {
            child.ParentCategoryId = null;
            MarkAsUpdated();
            //AddDomainEvent(new CategoryChildRemovedEvent(Id, child.Id));
        }
    }

    private bool IsDescendantOf(CategoryId potentialAncestorId)
    {
        var current = this;
        while (current.ParentCategoryId != null)
        {
            if (current.ParentCategoryId == potentialAncestorId)
                return true;

            // Здесь предполагается, что есть способ получить родительскую категорию
            // В реальном приложении это может быть через репозиторий или другим способом
            // current = GetParentCategory(current.ParentCategoryId);
            break;
        }
        return false;
    }
}
