namespace Domain.Catalog.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "грн")
    {
        RuleChecker.Check(new AmountMustBePositiveRule(amount));
        RuleChecker.Check(new CurrencyMustNotBeEmptyRule(currency));
        RuleChecker.Check(new CurrencyMustBeThreeLettersRule(currency));

        Amount = amount;
        Currency = currency;
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

