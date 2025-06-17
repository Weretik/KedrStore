namespace Domain.Common.Rules;

public static class RuleChecker
{
    public static void Check(IBusinessRule rule)
    {
        if (rule.IsBroken())
        {
            throw new BusinessRuleValidationException(rule);
        }
    }
}
