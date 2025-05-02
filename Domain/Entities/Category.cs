using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Category : BaseEntity<int>
    {
        public string Name { get; private set; } = default!;

        public int? ParentId { get; private set; }
        public Category? Parent { get; private set; }

        public ICollection<Category> SubCategories { get; private set; } = [];
        public ICollection<Product> Products { get; private set; } = [];

        public CategoryType Type { get; private set; }

        public Category(int id, string name, int? parentId = null, CategoryType type = 0)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Category name cannot be empty.");
            if (parentId != null && parentId.Value <= 0) throw new ArgumentException("Invalid parent category ID.");
            if (!Enum.IsDefined(typeof(CategoryType), type)) throw new ArgumentException("Invalid product type.");
            Id = id;
            Name = name;
            Type = type;
            ParentId = parentId;

        }
        private Category() { }
        public void ChangeName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) throw new ArgumentException("Category name cannot be empty.");
            Name = newName;
            MarkAsUpdated();
        }
        public void ChangeParent(int newParentId)
        {
            if (newParentId <= 0) throw new ArgumentException("Invalid parent category ID.");
            ParentId = newParentId;
            MarkAsUpdated();
        }
        public void ChangeType(CategoryType newType)
        {
            if (!Enum.IsDefined(typeof(CategoryType), newType)) throw new ArgumentException("Invalid category type.");
            Type = newType;
            MarkAsUpdated();
        }
        public void SoftDelete()
        {
            MarkAsDeleted();
        }
    }
    
}
