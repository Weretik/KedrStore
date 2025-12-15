using Domain.Catalog.Enumerations;
using Domain.Catalog.ValueObjects;

namespace Domain.Catalog.Entities;

public class Product : BaseAuditableEntity<ProductId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; } = null!;
    public ProductCategoryId CategoryId { get; private set; }
    public ProductType ProductType { get; private set; }
    public string Photo { get; private set; } = null!;
    public string? Sсheme {get; private set;}
    public decimal Stock { get; private set; }
    public int QuantityInPack { get; private set; }

    private readonly List<ProductPrice> _prices = new();
    public IReadOnlyCollection<ProductPrice> Prices => _prices;
    #endregion

    #region Constructors
    private Product() { }
    private Product(ProductId id, string name, ProductCategoryId categoryId, ProductType productType, string photo,
        DateTimeOffset createdDate, decimal stock = 0, string? sсheme = null, int qtyInPack = 0)
    {
        SetProductId(id);
        SetName(name);
        SetCategoryId(categoryId);
        SetProductType(productType);
        SetPhoto(photo);
        SetSсheme(sсheme);
        SetStock(stock);
        MarkAsCreated(createdDate);
        SetQuantityInPack(qtyInPack);
    }
    public static Product Create(ProductId id, string name, ProductCategoryId categoryId, ProductType productType,
        string photo, DateTimeOffset createdDate, decimal stock, int qtyInPack, string? scheme)
        => new(id, name, categoryId, productType, photo, createdDate, stock, scheme, qtyInPack);

    public void Update(string name, ProductCategoryId categoryId, ProductType productType, string photo,
        DateTimeOffset updatedDate, decimal stock,int qtyInPack, string? scheme)
    {
        SetName(name);
        SetCategoryId(categoryId);
        SetProductType(productType);
        SetPhoto(photo);
        SetSсheme(scheme);
        SetStock(stock);
        SetQuantityInPack(qtyInPack);
        MarkAsUpdated(updatedDate);
    }

    #endregion

    #region Validation & Setters
    private void SetProductId(ProductId id)
        => Id = Guard.Against.Default(id, nameof(id));

    private void SetName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.OutOfRange(name.Length, nameof(name), 1, 300);
        Name = name.Trim();
    }
    private void SetCategoryId(ProductCategoryId categoryId)
        => CategoryId = Guard.Against.Default(categoryId, nameof(categoryId));
    private void SetProductType(ProductType productType)
        => ProductType = Guard.Against.Default(productType, nameof(productType));
    private void SetPhoto(string photo)
        => Photo = Guard.Against.NullOrWhiteSpace(photo).Trim();

    private void SetSсheme(string? scheme)
    {
        if (scheme is null) Sсheme = null;
        else Sсheme = Guard.Against.NullOrWhiteSpace(scheme, nameof(scheme)).Trim();
    }
    private void SetStock(decimal stock)
        => Stock = Guard.Against.OutOfRange(stock, nameof(stock), 0, 10_000);

    private void SetQuantityInPack(int qtyInPack)
        => QuantityInPack = Guard.Against.Negative(qtyInPack);
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

    public void UpsertPrice(PriceType type, Money price)
    {
        var created = ProductPrice.Create(type, price);
        var existing = _prices.FirstOrDefault(p => p.PriceType == type);
        if (existing is not null) _prices.Remove(existing);
        _prices.Add(created);
    }

    public ProductPrice? GetPrice(PriceType type) => _prices.FirstOrDefault(x => x.PriceType == type);
    #endregion

}

