namespace Domain.Catalog.ValueObjects;

public sealed record ProductPrice
{
    #region Properties
    public PriceType PriceType { get; init; } = null!;
    private decimal _amount;
    private string _currencyIso = "UAH";
    public Money Price => new(_amount, Currency.FromCode(_currencyIso));
    #endregion

    #region Constructors
    private ProductPrice() { }
    private ProductPrice(PriceType type, decimal amount, string currencyIso)
    {

        PriceType = Guard.Against.Null(type, nameof(type));

        Guard.Against.Negative(amount, nameof(amount));
        Guard.Against.OutOfRange(amount, nameof(amount), 0.01m, 10_000m);

        Guard.Against.NullOrWhiteSpace(currencyIso, nameof(currencyIso));
        Guard.Against.InvalidInput(currencyIso, nameof(currencyIso),
            x => x.Length == 3, "Currency ISO code must be exactly 3 characters.");
        Guard.Against.InvalidInput(currencyIso, nameof(currencyIso),
            x => x.All(char.IsLetter), "Currency code must contain only letters.");

        _amount = amount;
        _currencyIso = currencyIso.ToUpper();
    }
    #endregion

    #region Factory Methods
    public static ProductPrice Create(int priceTypeId ,decimal amount, string currencyIso = "UAH")
        => new (GetPriceTypeFromId(priceTypeId), amount, currencyIso);

    public static ProductPrice Create(string priceTypeName, decimal amount, string currencyIso = "UAH")
        => new (GetPriceTypeFromName(priceTypeName), amount, currencyIso);

    public static ProductPrice Create(PriceType type, decimal amount, string currencyIso = "UAH")
        => new (type, amount, currencyIso);

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

}
