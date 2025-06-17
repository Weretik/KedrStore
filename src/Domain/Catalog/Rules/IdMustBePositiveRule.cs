namespace Domain.Catalog.Rules;

public class IdMustBePositiveRule(int Id) : BusinessRule
{
    public override string Message
        => "Id повинен бути більшим за нуль.";
    public override bool IsBroken()
        => Id < 0;

}
