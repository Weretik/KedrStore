namespace Domain.Catalog.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "грн")
    {
        Amount = Guard.Against.NegativeOrZero(amount);
        Currency = Guard.Against.NullOrWhiteSpace(currency).Trim();
        Guard.Against.InvalidInput(Currency, nameof(Currency),
            v => v.Length == 3 && v.All(char.IsLetter),
            "Currency must be a 3-letter code..");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount} {Currency}";
    public static explicit operator decimal(Money money) => money.Amount;
    public Money ChangeAmount(decimal newAmount) => new Money(newAmount, Currency);
    public Money ChangeCurrency(string newCurrency) => new Money(Amount, newCurrency);
}

