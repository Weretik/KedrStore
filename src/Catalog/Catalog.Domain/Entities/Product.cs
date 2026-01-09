using Catalog.Domain.Enumerations;
using Catalog.Domain.Errors;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities;

public class Product : BaseAuditableEntity<ProductId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; } = null!;
    public ProductCategoryId CategoryId { get; private set; }
    public string CategorySlug { get; private set; }
    public string Photo { get; private set; } = null!;
    public string? Sсheme {get; private set;}
    public decimal Stock { get; private set; }
    public bool IsSale { get; private set; }
    public bool IsNew { get; private set; }
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
        SetPhoto(photo);
        SetSсheme(sсheme);
        SetStock(stock);
        MarkAsCreated(createdDate);
        SetQuantityInPack(qtyInPack);
    }
    #endregion

    #region Factories
    public static Product Create(ProductId id, string name, ProductCategoryId categoryId, ProductType productType,
        string photo, DateTimeOffset createdDate, decimal stock, int qtyInPack, string? scheme)
        => new(id, name, categoryId, productType, photo, createdDate, stock, scheme, qtyInPack);

    public void Update(string name,  ProductCategoryId categoryId, ProductType productType, string photo,
        DateTimeOffset updatedDate, decimal stock,int qtyInPack, string? scheme)
    {
        SetName(name);
        SetCategoryId(categoryId);
        SetPhoto(photo);
        SetSсheme(scheme);
        SetStock(stock);
        SetQuantityInPack(qtyInPack);
        MarkAsUpdated(updatedDate);
    }
    #endregion

    #region Validation & Setters

    private void SetProductId(ProductId id)
    {
        if (id.Value <= 0) throw new DomainException(ProductErrors.IdRequired());
        Id = id;
    }
    private void SetCategoryId(ProductCategoryId categoryId)
    {
        if (categoryId.Value <= 0) throw new DomainException(ProductErrors.CategoryIdRequired());
        CategoryId = categoryId;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ProductErrors.NameRequired());

        var trimmed = name.Trim();

        if (trimmed.Length is < 1 or > 300)
            throw new DomainException(ProductErrors.NameLengthInvalid(trimmed.Length));

        Name = trimmed;
    }


    private void SetPhoto(string photo)
    {
        if (string.IsNullOrWhiteSpace(photo))
            throw new DomainException(ProductErrors.PhotoRequired());

        Photo = photo.Trim();
    }

    private void SetSсheme(string? scheme)
    {
        if (scheme is null)
        {
            Sсheme = null;
            return;
        }
        if (string.IsNullOrWhiteSpace(scheme))
            throw new DomainException(ProductErrors.SchemeInvalid());

        Sсheme = scheme.Trim();
    }
    private void SetStock(decimal stock)
    {
        if (stock < 0m || stock > 10_000m)
            throw new DomainException(ProductErrors.StockOutOfRange(stock));

        Stock = stock;
    }

    private void SetQuantityInPack(int qtyInPack)
    {
        if (qtyInPack < 0)
            throw new DomainException(ProductErrors.QuantityInPackNegative(qtyInPack));

        QuantityInPack = qtyInPack;
    }
    #endregion

    #region Markers
    public void MarkAsNew()
    {
        IsNew = true;
    }
    public void MarkAsSale()
    {
        IsSale = true;
    }
    public void RemoveSale()
    {
        IsSale = false;
    }
    public void RemoveNew()
    {
        IsNew = false;
    }
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

    #region OneCIntegraation
    public void UpdateStock(decimal stock) => SetStock(stock);
    #endregion
}

