namespace Domain.Catalog.Rules;

public class CurrencyMustNotBeEmptyRule(string currency)
    : BusinessRule
{
    public override string Message => "Валюта не повинна бути порожньою.";
    public override bool IsBroken() => string.IsNullOrWhiteSpace(currency);
}

