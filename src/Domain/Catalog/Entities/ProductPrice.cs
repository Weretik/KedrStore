namespace Domain.Catalog.Entities;

public sealed class ProductPrice : BaseEntity<ProductPriceId>
{
    #region Properties
    public ProductId ProductId { get; private set; }
    public PriceType PriceType { get; private set; } = null!;
    public Money PriceValue { get; private set; }
    #endregion

    #region Constructors
    private ProductPrice() { }
    private ProductPrice(ProductPriceId id, ProductId productId, PriceType type, decimal amount, string currencyIso)
    {
        SetProductPriceId(id);
        SetProductId(productId);
        SetPriceType(type);
        SetMoney(amount, currencyIso);
    }

    public static ProductPrice Create(ProductPriceId id, ProductId productId, decimal amount, string currencyIso = "UAH")
        => new (id, productId, GetPriceTypeFromId(id.Value), amount, currencyIso);

    public static ProductPrice Create(ProductPriceId id, ProductId productId, string priceTypeName, decimal amount, string currencyIso = "UAH")
        => new (id, productId, GetPriceTypeFromName(priceTypeName), amount, currencyIso);

    public static ProductPrice Create(ProductPriceId id, ProductId productId, PriceType type, decimal amount, string currencyIso = "UAH")
        => new (id, productId, type, amount, currencyIso);

    #endregion

    #region Validation & Setters
    private void SetProductPriceId(ProductPriceId id) => Id = Guard.Against.Default(id, nameof(id));
    private void SetProductId(ProductId productId) => ProductId = Guard.Against.Default(productId, nameof(productId));
    private void SetPriceType(PriceType type) => PriceType = Guard.Against.Null(type, nameof(type));
    private void SetMoney(decimal amount, string currencyIso)
    {
        Guard.Against.Negative(amount, nameof(amount));
        Guard.Against.OutOfRange(amount, nameof(amount), 0.01m, 10_000m);
        Guard.Against.NullOrWhiteSpace(currencyIso, nameof(currencyIso));

        Guard.Against.InvalidInput(currencyIso, nameof(currencyIso),
            x => x.Length == 3, "Currency ISO code must be exactly 3 characters.");
        Guard.Against.InvalidInput(currencyIso, nameof(currencyIso),
            x => x.All(char.IsLetter), "Currency code must contain only letters.");

        PriceValue = new Money(amount, currencyIso);
    }
    #endregion

    #region Validation &  static Getters
    private static PriceType GetPriceTypeFromId(int priceTypeValue)
    {
        Guard.Against.OutOfRange(priceTypeValue, nameof(priceTypeValue), 1, 12);

        return PriceType.FromValue(priceTypeValue);
    }
    private static PriceType GetPriceTypeFromName(string priceTypeName)
    {
        Guard.Against.NullOrWhiteSpace(priceTypeName, nameof(priceTypeName));

        Guard.Against.InvalidInput(priceTypeName, nameof(priceTypeName),
            x => PriceType.TryFromName(x.Trim(), out _),
            $"Unknown PriceType name: {priceTypeName}");

        return PriceType.FromName(priceTypeName.Trim(), ignoreCase: true);
    }
    #endregion

    #region Update
    public void UpdateMoney(decimal amount, string currencyIso = "UAH")
    {
        SetMoney(amount, currencyIso);
    }
    #endregion
}
