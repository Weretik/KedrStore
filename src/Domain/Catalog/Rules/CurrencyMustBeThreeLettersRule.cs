namespace Domain.Catalog.Rules;

public class CurrencyMustBeThreeLettersRule(string currency)
    : BusinessRule
{
    public override string Message => "Валюта повинна містити три літери.";
    public override bool IsBroken() =>  currency.Length != 3;
}
