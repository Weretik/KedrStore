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
    private Product(ProductId id, string name, string manufacturer, Money price, CategoryId categoryId, string photo)
    {
        SetProductId(id);
        SetName(name);
        SetManufacturer(manufacturer);
        SetMoney(price);
        SetCategoryId(categoryId);
        SetPhoto(photo);

        AddCreatedEvent();
    }
    public static Product Create(ProductId id, string name, string manufacturer, Money price,
        CategoryId categoryId, string photo)
    {
        return new Product(id, name, manufacturer, price, categoryId, photo);
    }

    #endregion

    #region Validation & Setters

    private void SetProductId(ProductId id)
    {
        RuleChecker.Check(new IdMustNotBeNullRule(id));
        Id = id;
    }
    private void SetName(string name)
    {
        RuleChecker.Check(new NameMustNotBeEmptyRule(name));
        Name = name;
    }
    private void SetManufacturer(string manufacturer)
    {
        RuleChecker.Check(new ManufacturerMustNotBeEmptyRule(manufacturer));
        Manufacturer = manufacturer;
    }
    private void SetMoney(Money price)
    {
        RuleChecker.Check(new MoneyMustNotBeNullRule(price));
        Price = price;
    }
    private void SetCategoryId(CategoryId categoryId)
    {
        RuleChecker.Check(new IdMustNotBeNullRule(categoryId));
        CategoryId = categoryId;
    }
    private void SetPhoto(string photo)
    {
        RuleChecker.Check(new PhotoMustNotBeEmptyRule(photo));
        Photo = photo;
    }

    #endregion

    #region Update
    public void Update(string name, string manufacturer, Money price, CategoryId categoryId, string photo)
    {
        ChangeName(name);
        ChangeManufacturer(manufacturer);
        ChangeMoney(price);
        ChangeCategory(categoryId);
        ChangePhoto(photo);
    }

    public void ChangeName(string name)
    {
        SetName(name);
        MarkAsUpdated();
    }

    public void ChangeManufacturer(string manufacturer)
    {
        SetManufacturer(manufacturer);
        MarkAsUpdated();
    }

    public void ChangeMoney(Money price)
    {
        SetMoney(price);
        MarkAsUpdated();
    }

    public void ChangeCategory(CategoryId categoryId)
    {
        SetCategoryId(categoryId);
        MarkAsUpdated();
    }

    public void ChangePhoto(string photo)
    {
        SetPhoto(photo);
        MarkAsUpdated();
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

    #region Domain Events

    private void AddCreatedEvent() => AddDomainEvent(new ProductCreatedEvent(Id));

    #endregion
}

