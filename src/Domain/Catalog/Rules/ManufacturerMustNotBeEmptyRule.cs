namespace Domain.Catalog.Rules;

public class ManufacturerMustNotBeEmptyRule(string manufacturer)
    : BusinessRule
{
    public override string Message => "Виробник не може бути порожнім.";
    public override bool IsBroken() => string.IsNullOrWhiteSpace(manufacturer);
}
