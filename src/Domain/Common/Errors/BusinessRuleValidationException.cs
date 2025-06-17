namespace Domain.Common.Errors;

public class BusinessRuleValidationException(IBusinessRule brokenRule)
    : Exception(brokenRule.Message)
{
    public IBusinessRule BrokenRule { get; } = brokenRule;

    public override string ToString()
    {
        return $"{BrokenRule.GetType().Name}: {BrokenRule.Message}";
    }
}
