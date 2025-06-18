namespace Domain.Catalog.Rules;

public class MoneyMustNotBeNullRule(Money price)
    : BusinessRule
{
    public override string Message => "Ціна не може бути null.";
    public override bool IsBroken() => price == null;
}
