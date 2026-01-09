using Catalog.Domain.Errors;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities;

public class ProductPrice : BaseEntity<ProductPriceId>, IAggregateRoot
{
    #region Properties
    public ProductId ProductId { get; set; }
    public PriceTypeId PriceTypeId { get; private set; }
    public decimal Amount { get; private set; }
    public string CurrencyIso { get; private set; } = "UAH";
    public Money Price => new(Amount, Currency.FromCode(CurrencyIso));
    #endregion

    #region Constructors
    private ProductPrice() { }

    private ProductPrice(ProductPriceId id, ProductId productId, PriceTypeId priceTypeId, Money price)
    {
        SetId(id);
        SetProductId(productId);
        SetPriceTypeId(priceTypeId);
        SetMoney(price);
    }
    #endregion

    #region Factories

    public static ProductPrice Create(ProductPriceId id, ProductId productId, PriceTypeId priceTypeId, Money price)
        => new();

    public void Update(ProductId productId, PriceTypeId priceTypeId, Money price)
    {

    }
    #endregion

    #region Validation & Setters
    private void SetId(ProductPriceId id)
    {
        if (id.Value <= 0) throw new DomainException(ProductPriceErrors.IdRequired());
        Id = id;
    }
    private void SetProductId(ProductId productId)
    {
        if (productId.Value <= 0) throw new DomainException(ProductPriceErrors.ProductIdRequired());
        ProductId = productId;
    }
    private void SetPriceTypeId(PriceTypeId priceTypeId)
    {
        if (priceTypeId.Value <= 0) throw new DomainException(ProductPriceErrors.PriceTypeRequired());
        PriceTypeId = priceTypeId;
    }

    private void SetMoney(Money price)
    {
        if (price.Amount < 0m)
            throw new DomainException(ProductPriceErrors.AmountNegative(price.Amount));

        if (price.Amount > 100_000m)
            throw new DomainException(ProductPriceErrors.AmountOutOfRange(price.Amount));

        var code = price.Currency.Code;

        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(ProductPriceErrors.CurrencyCodeRequired());

        code = code.Trim();

        if (code.Length != 3)
            throw new DomainException(ProductPriceErrors.CurrencyCodeLengthInvalid(code));

        if (!code.All(char.IsLetter))
            throw new DomainException(ProductPriceErrors.CurrencyCodeNonLetters(code));

        Amount = price.Amount;
        CurrencyIso = code.ToUpperInvariant();
    }
    #endregion
}
