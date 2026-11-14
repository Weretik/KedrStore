using Catalog.Domain.PriceTypes;

namespace Catalog.Domain.Products.Prices;

public class ProductPrice : BaseEntity<int>, IAggregateRoot
{
    #region Properties
    public string ProductTypeIdOneC { get; private set; } = null!;
    public ProductId ProductId { get; private set; }
    public PriceTypeId PriceTypeId { get; private set; }
    public decimal Amount { get; private set; }
    public string CurrencyIso { get; private set; } = "UAH";
    public Money Price => new(Amount, Currency.FromCode(CurrencyIso));
    #endregion

    public const decimal MaximumAmount = 5_000_000m;

    #region Constructors
    private ProductPrice() { }
    #endregion

    #region Factories
    public static DomainResult<ProductPrice> Create(string productTypeIdOneC, ProductId productId,
        PriceTypeId priceTypeId, Money price)
    {
        var created = new ProductPrice { ProductTypeIdOneC = productTypeIdOneC };
        var updated = created.Update(productId, priceTypeId, price);
        return updated.IsFailure ? updated.Error! : created;
    }

    public DomainResult Update(ProductId productId, PriceTypeId priceTypeId, Money price)
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
