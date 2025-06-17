namespace Domain.Common.Exception;

public sealed class BusinessRuleValidationException(IBusinessRule rule)
    : System.Exception(rule.Message)
{
    public IBusinessRule BrokenRule { get; } = rule;

    public string Code => BrokenRule.GetType().Name;

    public override string ToString() => $"{Code}: {BrokenRule.Message}";
}
