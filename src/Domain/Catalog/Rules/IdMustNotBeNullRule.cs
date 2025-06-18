namespace Domain.Catalog.Rules;

public class IdMustNotBeNullRule(EntityId id)
    : BusinessRule
{
    public override string Message => "Id не може бути null.";
    public override bool IsBroken() => id == null;
}
