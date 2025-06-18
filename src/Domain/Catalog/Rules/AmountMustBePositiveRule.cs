namespace Domain.Catalog.Rules;

public class AmountMustBePositiveRule(decimal amount)
    : BusinessRule
{
    public override string Message => "Сума має бути позитивною.";
    public override bool IsBroken() => amount <= 0;
}
