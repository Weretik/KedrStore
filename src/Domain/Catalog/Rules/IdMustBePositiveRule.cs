namespace Domain.Catalog.Rules;

public class IdMustBePositiveRule(int id)
    : BusinessRule
{
    public override string Message => "Id повинен бути більшим за нуль.";
    public override bool IsBroken() => id < 0;

}
