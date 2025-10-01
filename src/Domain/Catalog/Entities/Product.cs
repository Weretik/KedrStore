namespace Domain.Catalog.Entities;

public class Product : BaseAuditableEntity<ProductId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public CategoryId CategoryId { get; private set; }
    public string Photo { get; private set; } = null!;
    #endregion

    #region Constructors
    private Product() { }
    private Product(ProductId id, string name, Money price, CategoryId categoryId, string photo, DateTime createdDate)
    {
        SetProductId(id);
        SetName(name);
        SetMoney(price);
        SetCategoryId(categoryId);
        SetPhoto(photo);
        MarkAsCreated(createdDate);
    }
    public static Product Create(ProductId id, string name, Money price, CategoryId categoryId, string photo, DateTime createdDate)
        => new Product(id, name, price, categoryId, photo, createdDate);

    #endregion

    #region Validation & Setters
    private void SetProductId(ProductId id) => Id = Guard.Against.Default(id, nameof(id));
    private void SetName(string name) => Name = Guard.Against.NullOrWhiteSpace(name, nameof(name)).Trim();
    private void SetMoney(Money price) => Price = Guard.Against.Null(price, nameof(price)) ;
    private void SetCategoryId(CategoryId categoryId) => CategoryId = Guard.Against.Null(categoryId, nameof(categoryId));
    private void SetPhoto(string photo) => Photo = Guard.Against.NullOrWhiteSpace(photo).Trim();
    #endregion

    #region Update
    public void Update(string name, string manufacturer, Money price, CategoryId categoryId, string photo, DateTime updatedDate)
    {
        SetName(name);
        SetMoney(price);
        SetCategoryId(categoryId);
        SetPhoto(photo);
        MarkAsUpdated(updatedDate);

    }
    #endregion

}

