namespace Domain.Catalog.Entities;

public class Product : BaseAuditableEntity<ProductId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; } = null!;
    public ProductCategoryId CategoryId { get; private set; }
    public ProductType ProductType { get; private set; }
    public string Photo { get; private set; } = null!;
    public decimal Stock { get; private set; }

    private readonly List<ProductPrice> _prices = new();
    public IReadOnlyCollection<ProductPrice> Prices => _prices;
    #endregion

    #region Constructors
    private Product() { }
    private Product(ProductId id, string name, ProductCategoryId categoryId, ProductType productType,
        string photo, DateTime createdDate, decimal stock = 0)
    {
        SetProductId(id);
        SetName(name);
        SetCategoryId(categoryId);
        SetProductType(productType);
        SetPhoto(photo);
        SetStock(stock);
        MarkAsCreated(createdDate);
    }
    public static Product Create(ProductId id, string name, ProductCategoryId categoryId,ProductType productType,
        string photo, DateTime createdDate)
        => new(id, name, categoryId, productType, photo, createdDate);

    #endregion

    #region Validation & Setters
    private void SetProductId(ProductId id) => Id = Guard.Against.Default(id, nameof(id));

    private void SetName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.OutOfRange(name.Length, nameof(name), 1, 300);
        Name = name.Trim();
    }
    private void SetCategoryId(ProductCategoryId categoryId) => CategoryId = Guard.Against.Default(categoryId, nameof(categoryId));
    private void SetProductType(ProductType productType) => ProductType = Guard.Against.Null(productType, nameof(productType));
    private void SetPhoto(string photo) => Photo = Guard.Against.NullOrWhiteSpace(photo).Trim();
    private void SetStock(decimal stock) => Stock = Guard.Against.OutOfRange(stock, nameof(stock), 0, 1_000);
    #endregion

    #region Price API
    public void AddOrReplacePrices(IEnumerable<ProductPrice> prices)
    {
        foreach (var poductPrice in prices)
        {
            var index = _prices.FindIndex(x => x.PriceType == poductPrice.PriceType);
            if (index >= 0) _prices[index] = poductPrice;
            else _prices.Add(poductPrice);
        }
    }

    public void ReplaceAllPrices(IEnumerable<ProductPrice> prices)
    {
        _prices.Clear();
        _prices.AddRange(prices);
    }

    public void UpsertPrice(PriceType type, decimal amount, string iso = "UAH")
    {
        var created = ProductPrice.Create(type, amount, iso);
        var existing = _prices.FirstOrDefault(p => p.PriceType == type);
        if (existing is not null) _prices.Remove(existing);
        _prices.Add(created);
    }

    public ProductPrice? GetPrice(PriceType type) => _prices.FirstOrDefault(x => x.PriceType == type);
    #endregion

}

