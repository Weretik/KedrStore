using Catalog.Domain.PriceTypes;

namespace Catalog.Domain.Products.Prices;

public class ProductPrice : BaseEntity<ProductPriceId>, IAggregateRoot
{
    #region Properties
    public string ProductTypeIdOneC { get; private set; } = null!;
    public ProductId ProductId { get; private set; }
    public PriceTypeId PriceTypeId { get; private set; }
    public decimal Amount { get; private set; }
    public string CurrencyIso { get; private set; } = "UAH";
    public Money Price => new(Amount, Currency.FromCode(CurrencyIso));

    /// <summary>
    /// When the owner's price file locked this price from the 1C sync (spec catalog/010); null while 1C owns it.
    /// </summary>
    public DateTimeOffset? LockedFromOneCAt { get; private set; }

    public bool IsLockedFromOneC => LockedFromOneCAt is not null;

    /// <summary>The 1C sync removes a price it no longer sends only while the price is not locked.</summary>
    public bool CanBeRemovedByOneC => !IsLockedFromOneC;
    #endregion

    public const decimal MaximumAmount = 5_000_000m;

    /// <summary>Prices in the owner's file are retail prices in hryvnias.</summary>
    public const string FileCurrency = "UAH";

    #region Constructors
    private ProductPrice() { }
    #endregion

    #region Factories
    public static DomainResult<ProductPrice> Create(string productTypeIdOneC, ProductId productId,
        PriceTypeId priceTypeId, Money price)
    {
        var created = new ProductPrice { ProductTypeIdOneC = productTypeIdOneC };
        var updated = created.Apply(productId, priceTypeId, price);
        return updated.IsFailure ? updated.Error! : created;
    }

    /// <summary>A retail price for a product that had none, from the owner's file (spec catalog/010 RP-03, RP-04).</summary>
    public static DomainResult<ProductPrice> CreateFromFile(string productTypeIdOneC, ProductId productId,
        PriceTypeId priceTypeId, decimal amount, bool lockFromOneC, DateTimeOffset now)
    {
        var created = Create(productTypeIdOneC, productId, priceTypeId, FileMoney(amount));
        if (created.IsFailure)
            return created.Error!;

        created.Value.LockedFromOneCAt = lockFromOneC ? now : null;
        return created;
    }

    /// <summary>
    /// The 1C sync's change (spec catalog/010 RP-05): a price locked by the owner's file keeps its amount.
    /// </summary>
    public DomainResult UpdateFromOneC(Money price)
        => IsLockedFromOneC ? ProductPriceErrors.LockedFromOneC() : Apply(ProductId, PriceTypeId, price);

    /// <summary>
    /// The price from the owner's file in hryvnias, rounded to kopiykas, locked from 1C or left to it
    /// (spec catalog/010 RP-03, RP-04). The value tells whether the amount changed.
    /// </summary>
    public DomainResult<bool> SetFromFile(decimal amount, bool lockFromOneC, DateTimeOffset now)
    {
        var money = FileMoney(amount);
        var changed = Amount != money.Amount || CurrencyIso != FileCurrency;
        var applied = Apply(ProductId, PriceTypeId, money);
        if (applied.IsFailure)
            return applied.Error!;

        LockedFromOneCAt = lockFromOneC ? now : null;
        return changed;
    }

    /// <summary>Gives the price back to the 1C sync (spec catalog/010 RP-06); its amount stays until the next sync.</summary>
    public void UnlockFromOneC() => LockedFromOneCAt = null;

    private static Money FileMoney(decimal amount)
        => new(decimal.Round(amount, 2, MidpointRounding.AwayFromZero), FileCurrency);

    private DomainResult Apply(ProductId productId, PriceTypeId priceTypeId, Money price)
    {
        var checks = new DomainChecks();
        if (productId.Value <= 0)
            checks.Check(ProductPriceErrors.ProductIdRequired());
        if (priceTypeId.Value <= 0)
            checks.Check(ProductPriceErrors.PriceTypeRequired());
        var currency = checks.Take(CheckMoney(price));
        if (checks.Error is { } error)
            return error;

        ProductId = productId;
        PriceTypeId = priceTypeId;
        Amount = price.Amount;
        CurrencyIso = currency;
        return DomainResult.Success();
    }
    #endregion

    #region Validation
    /// <summary>The checked currency code, upper case.</summary>
    private static DomainResult<string> CheckMoney(Money price)
    {
        if (price.Amount < 0m)
            return ProductPriceErrors.AmountNegative();
        if (price.Amount > MaximumAmount)
            return ProductPriceErrors.AmountOutOfRange();

        var code = price.Currency.Code?.Trim() ?? string.Empty;
        if (code.Length == 0)
            return ProductPriceErrors.CurrencyCodeRequired();
        if (code.Length != 3 || !code.All(char.IsLetter))
            return ProductPriceErrors.CurrencyCodeInvalid();
        return code.ToUpperInvariant();
    }
    #endregion
}
