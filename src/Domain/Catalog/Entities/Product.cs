namespace Domain.Catalog.Entities;

public class Product : BaseEntity<ProductId>, IAggregateRoot
{
    #region Properties
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
    #endregion

    #region Constructors

    private Product() { }
    private Product(ProductId id, string name, string manufacturer, Money price, CategoryId categoryId,
        string photo, DateTime createdDate)
    {
        SetProductId(id);
        SetName(name);
        SetManufacturer(manufacturer);
        SetMoney(price);
        SetCategoryId(categoryId);
        SetPhoto(photo);
        MarkAsCreated(createdDate);
    }
    public static Product Create(ProductId id, string name, string manufacturer, Money price,
        CategoryId categoryId, string photo, DateTime createdDate)
    {
        return new Product(id, name, manufacturer, price, categoryId, photo, createdDate);
    }

    #endregion

    #region Validation & Setters

    private void SetProductId(ProductId id)
    {
        Id = Guard.Against.Null(id, nameof(id));
    }
    private void SetName(string name)
    {
        Name = Guard.Against.NullOrWhiteSpace(name, nameof(name)).Trim();
    }
    private void SetManufacturer(string manufacturer)
    {
        Manufacturer = Guard.Against.NullOrWhiteSpace(manufacturer, nameof(manufacturer));
    }
    private void SetMoney(Money price)
    {
        Price = Guard.Against.Null(price, nameof(price)) ;
    }
    private void SetCategoryId(CategoryId categoryId)
    {
        CategoryId = Guard.Against.Null(categoryId, nameof(categoryId));
    }
    private void SetPhoto(string photo)
    {
        Photo = Guard.Against.NullOrWhiteSpace(photo).Trim();
    }

    #endregion

    #region Update
    public void Update(string name, string manufacturer, Money price, CategoryId categoryId, string photo,
        DateTime updatedDate)
    {
        ChangeName(name, updatedDate);
        ChangeManufacturer(manufacturer, updatedDate);
        ChangeMoney(price, updatedDate);
        ChangeCategory(categoryId, updatedDate);
        ChangePhoto(photo, updatedDate);
    }

    public void ChangeName(string name, DateTime updatedDate)
    {
        SetName(name);
        MarkAsUpdated(updatedDate);
    }

    public void ChangeManufacturer(string manufacturer, DateTime updatedDate)
    {
        SetManufacturer(manufacturer);
        MarkAsUpdated(updatedDate);
    }

    public void ChangeMoney(Money price, DateTime updatedDate)
    {
        SetMoney(price);
        MarkAsUpdated(updatedDate);
    }

    public void ChangeCategory(CategoryId categoryId, DateTime updatedDate)
    {
        SetCategoryId(categoryId);
        MarkAsUpdated(updatedDate);
    }

    public void ChangePhoto(string photo, DateTime updatedDate)
    {
        SetPhoto(photo);
        MarkAsUpdated(updatedDate);
    }

    /*
    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Количество товара не может быть отрицательным", nameof(quantity));

        StockQuantity = quantity;
    }

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

    #endregion

}

