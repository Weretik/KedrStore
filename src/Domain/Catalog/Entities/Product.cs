namespace Domain.Catalog.Entities;

public class Product : BaseAuditableEntity<ProductId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; } = null!;
    public CategoryId CategoryId { get; private set; }
    public string Photo { get; private set; } = null!;
    public decimal Stock { get; private set; }

    private readonly List<ProductPrice> _prices = new();
    public IReadOnlyCollection<ProductPrice> Prices => _prices;
    #endregion

    #region Constructors
    private Product() { }
    private Product(ProductId id, string name, CategoryId categoryId, string photo, DateTime createdDate, decimal stock = 0)
    {
        SetProductId(id);
        SetName(name);
        SetCategoryId(categoryId);
        SetPhoto(photo);
        SetStock(stock);
        MarkAsCreated(createdDate);
    }
    public static Product Create(ProductId id, string name, Money price, CategoryId categoryId, string photo, DateTime createdDate)
        => new(id, name, categoryId, photo, createdDate);

    #endregion

    #region Validation & Setters
    private void SetProductId(ProductId id) => Id = Guard.Against.Default(id, nameof(id));

    private void SetName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.OutOfRange(name.Length, nameof(name), 1, 256);
        Name = name.Trim();
    }
    private void SetCategoryId(CategoryId categoryId) => CategoryId = Guard.Against.Default(categoryId, nameof(categoryId));
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
    public ProductPrice? GetPrice(PriceType type) => _prices.FirstOrDefault(x => x.PriceType == type);
    public void UpsertPrice(ProductPriceId priceId, string priceTypeName, decimal amount, string currencyIso = "UAH")
    {
        var type = PriceType.FromName(priceTypeName.Trim(), ignoreCase: true);
        var existingPrice = _prices.FirstOrDefault(p => p.PriceType == type);

        if (existingPrice is not null)
        {
            existingPrice.UpdateMoney(amount, currencyIso);
            return;
        }

        var createdPrice = ProductPrice.Create(
            id: priceId,
            productId: Id,
            priceTypeName: priceTypeName,
            amount: amount,
            currencyIso: currencyIso);

        _prices.Add(createdPrice);
    }
    #endregion

}

