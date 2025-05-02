using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Product : BaseEntity<int>
    {
        public string Name { get; private set; } = default!;
        public string Manufacturer { get; private set; } = default!;
        public decimal Price { get; private set; }
        public string PhotoUrl { get; private set; } = default!;

        public int CategoryId { get; private set; }
        public Category Category { get; private set; } = default!;

        public ProductType Type { get; private set; }
        public Product(int id, string name, string manufacturer, decimal price, string photoUrl, int categoryId, ProductType type = 0)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Product name cannot be empty.");
            if (string.IsNullOrWhiteSpace(manufacturer)) throw new ArgumentException("Product manufacture cannot be empty.");
            if (price <= 0) throw new ArgumentException("Product price must be greater than zero.");
            if (string.IsNullOrWhiteSpace(photoUrl)) throw new ArgumentException("Product photo URL cannot be empty.");
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID.");
            Id = id;
            Name = name;
            Manufacturer = manufacturer;
            Price = price;
            PhotoUrl = photoUrl;
            CategoryId = categoryId;
            Type = type;
        }
        private Product() { }
        public void ChangeName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) throw new ArgumentException("Product name cannot be empty.");
            Name = newName;
            MarkAsUpdated();
        }
        public void ChangeManufacturer(string newManufacturer)
        {
            if (string.IsNullOrWhiteSpace(newManufacturer)) throw new ArgumentException("Product manufacture cannot be empty.");
            Manufacturer = newManufacturer;
            MarkAsUpdated();
        }
        public void ChangePhotoUrl(string newPhotoUrl)
        {
            if (string.IsNullOrWhiteSpace(newPhotoUrl)) throw new ArgumentException("Product photo URL cannot be empty.");
            PhotoUrl = newPhotoUrl;
            MarkAsUpdated();
        }
        public void ChangePrice(decimal newPrice)
        {
            if (newPrice <= 0) throw new ArgumentException("Product price must be greater than zero.");
            Price = newPrice;
            MarkAsUpdated();
        }
        public void ChangeCategory(int newCategoryId)
        {
            if (newCategoryId <= 0) throw new ArgumentException("Invalid category ID.");
            CategoryId = newCategoryId;
            MarkAsUpdated();
        }
        public void ChangType(ProductType newType)
        {
            if (!Enum.IsDefined(typeof(ProductType), newType)) throw new ArgumentException("Invalid product type.");
       
            Type = newType;
            MarkAsUpdated();
        }
        public void SoftDelete()
        {
            MarkAsDeleted();
        }
    }
}
