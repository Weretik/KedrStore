using Domain.Catalog.Enumerations;

namespace Domain.Catalog.ValueObjects;

public sealed record ProductPrice
{
    #region Properties
    public PriceType PriceType { get; init; } = null!;
    public decimal Amount { get; init; }
    public string CurrencyIso { get; init; } = "UAH";
    public Money Price => new(Amount, Currency.FromCode(CurrencyIso));
    #endregion

    #region Constructors
    private ProductPrice() { }
    private ProductPrice(PriceType type, Money price)
    {

        PriceType = Guard.Against.Null(type, nameof(type));

        Guard.Against.Negative(price.Amount, nameof(price.Amount));
        Guard.Against.OutOfRange(price.Amount, nameof(price.Amount), 0.00m, 100_000m);

        Guard.Against.NullOrWhiteSpace(price.Currency.Code, nameof(price.Currency.Code));
        Guard.Against.InvalidInput(price.Currency.Code, nameof(price.Currency.Code),
            x => x.Length == 3, "Currency ISO code must be exactly 3 characters.");
        Guard.Against.InvalidInput(price.Currency.Code, nameof(price.Currency.Code),
            x => x.All(char.IsLetter), "Currency code must contain only letters.");

        Amount = price.Amount;
        CurrencyIso = price.Currency.Code;
    }
    #endregion

    #region Factory Methods
    public static ProductPrice Create(int priceTypeId, Money price)
        => new (GetPriceTypeFromId(priceTypeId), price);

    public static ProductPrice Create(string priceTypeName, Money price)
        => new (GetPriceTypeFromName(priceTypeName), price);

    public static ProductPrice Create(PriceType type, Money price)
        => new (type, price);

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
