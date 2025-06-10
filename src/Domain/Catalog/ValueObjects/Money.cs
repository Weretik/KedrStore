using Domain.Common;

namespace Domain.Catalog.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money() { }

    public Money(decimal amount, string currency = "грн")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));
        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter code", nameof(currency));

        Amount = amount;
        Currency = currency;
    }
    //TODO при централизации ошибок поменять throw
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public Money ChangeAmount(decimal newAmount) => new Money(newAmount, Currency);
    public Money ChangeCurrency(string newCurrency) => new Money(Amount, newCurrency);
}

