namespace Domain.Catalog.Entities;

public class Product : BaseEntity<ProductId>, IAggregateRoot
{
    public string Name { get; private set; } = null!;
    public string Manufacturer { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public CategoryId CategoryId { get; private set; } = null!;
    public string Photo { get; private set; } = null!;

    /*
        //private readonly List<ProductAttribute> _attributes = new();
        //private readonly List<ProductImage> _images = new();

        //public string? Description { get; private set; }
        //public string? ShortDescription { get; private set; }

        //public int StockQuantity { get; private set; }

        //public bool IsActive { get; private set; }
        //public IReadOnlyList<ProductAttribute> Attributes => _attributes.AsReadOnly();
        //public IReadOnlyList<ProductImage> Images => _images.AsReadOnly();
    */

    private Product() { }
    private Product(ProductId id, string name, string manufacturer, Money price, CategoryId categoryId, string photo)
    {
        Id = id;
        Name = name;
        Manufacturer = manufacturer;
        Price = price;
        CategoryId = categoryId;
        Photo = photo;

        AddDomainEvent(new ProductCreatedEvent(id));
    }
    /*
     * Возможно, стоит также добавить события для других важных изменений состояния продукта, например:
     * ProductUpdatedEvent (при обновлении основных данных)
     * ProductStockChangedEvent (при изменении количества)
     */

    private static void ValidateId(ProductId id)
    {
        if (id.Value <= 0)
            throw new ArgumentException("ProductId must be greater than zero", nameof(id));
        if (id == null)
            throw new ArgumentNullException(nameof(id), "ProductId cannot be null");
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
    }

    private static void ValidateManufacturer(string manufacturer)
    {
        if (string.IsNullOrWhiteSpace(manufacturer))
            throw new ArgumentException("Manufacturer is required", nameof(manufacturer));
    }

    private static void ValidatePrice(Money price)
    {
        if (price == null)
            throw new ArgumentNullException(nameof(price), "Price cannot be null");
    }
    private static void ValidateCategoryId(CategoryId categoryId)
    {
        if (categoryId.Value <= 0)
            throw new ArgumentException("CategoryId must be greater than zero", nameof(categoryId));
        if (categoryId == null)
            throw new ArgumentNullException(nameof(categoryId), "CategoryId cannot be null");
    }

    private static void ValidatePhoto(string photo)
    {
        if (string.IsNullOrWhiteSpace(photo))
            throw new ArgumentException("Photo is required", nameof(photo));
    }

    public static Product Create(ProductId id, string name, string manufacturer, Money price,
        CategoryId categoryId, string photo)
    {
        ValidateId(id);
        ValidateName(name);
        ValidateManufacturer(manufacturer);
        ValidateCategoryId(categoryId);
        ValidatePhoto(photo);

        return new Product(id, name, manufacturer, price, categoryId, photo);
    }

    public void Update(
        string name,
        string manufacturer,
        Money price,
        CategoryId categoryId,
        string photo)
    {
        ValidateName(name);
        ValidateManufacturer(manufacturer);
        ValidateCategoryId(categoryId);
        ValidatePhoto(photo);
        ValidatePrice(price);

        Name = name;
        Manufacturer = manufacturer;
        Price = price;
        CategoryId = categoryId;
        Photo = photo;

        MarkAsUpdated();
    }

    public void ChangeName(string name)
    {
        ValidateName(name);
        Name = name;
        MarkAsUpdated();
    }

    public void ChangeManufacturer(string manufacturer)
    {
        ValidateManufacturer(manufacturer);
        Manufacturer = manufacturer;
        MarkAsUpdated();
    }

    public void ChangeMoney(Money price)
    {
        ValidatePrice(price);
        Price = price;
        MarkAsUpdated();
    }

    public void ChangeCategory(CategoryId categoryId)
    {
        ValidateCategoryId(categoryId);
        CategoryId = categoryId;
        MarkAsUpdated();
    }

    public void ChangePhoto(string photo)
    {
        ValidatePhoto(photo);
        Photo = photo;
        MarkAsUpdated();
    }
    /*
    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Количество товара не может быть отрицательным", nameof(quantity));

        StockQuantity = quantity;
    }
    */
    /*
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void AddAttribute(ProductAttribute attribute)
    {
        _attributes.Add(attribute);
    }

    public void AddImage(ProductImage image)
    {
        _images.Add(image);
    }
    */

}

