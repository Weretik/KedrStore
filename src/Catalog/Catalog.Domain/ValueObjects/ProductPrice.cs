using Catalog.Domain.Enumerations;
using Catalog.Domain.Errors;

namespace Catalog.Domain.ValueObjects;

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
        if (type is null)
            throw new DomainException(ProductPriceErrors.PriceTypeRequired());

        // Amount
        if (price.Amount < 0m)
            throw new DomainException(ProductPriceErrors.AmountNegative(price.Amount));

        if (price.Amount > 100_000m)
            throw new DomainException(ProductPriceErrors.AmountOutOfRange(price.Amount));

        // Currency code
        var code = price.Currency.Code;

        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(ProductPriceErrors.CurrencyCodeRequired());

        code = code.Trim();

        if (code.Length != 3)
            throw new DomainException(ProductPriceErrors.CurrencyCodeLengthInvalid(code));

        if (!code.All(char.IsLetter))
            throw new DomainException(ProductPriceErrors.CurrencyCodeNonLetters(code));

        PriceType = type;
        Amount = price.Amount;
        CurrencyIso = code.ToUpperInvariant();
    }
    #endregion

    #region Factory Methods
    public static ProductPrice Create(int priceTypeId, Money price)
        => new(GetPriceTypeFromId(priceTypeId), price);

    public static ProductPrice Create(string priceTypeName, Money price)
        => new(GetPriceTypeFromName(priceTypeName), price);

    public static ProductPrice Create(PriceType type, Money price)
        => new(type, price);
    #endregion

    #region Validation & static Getters
    private static PriceType GetPriceTypeFromId(int priceTypeValue)
    {
        if (priceTypeValue < 1 || priceTypeValue > 14)
            throw new DomainException(ProductPriceErrors.PriceTypeIdOutOfRange(priceTypeValue));

        return PriceType.FromValue(priceTypeValue);
    }

    private static PriceType GetPriceTypeFromName(string priceTypeName)
    {
        if (string.IsNullOrWhiteSpace(priceTypeName))
            throw new DomainException(ProductPriceErrors.PriceTypeNameRequired());

        var trimmed = priceTypeName.Trim();

        if (!PriceType.TryFromName(trimmed, out _))
            throw new DomainException(ProductPriceErrors.UnknownPriceTypeName(trimmed));

        return PriceType.FromName(trimmed, ignoreCase: true);
    }
    #endregion
}
