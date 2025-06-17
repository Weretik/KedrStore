namespace Domain.Common.Rules;

public abstract class BusinessRule : IBusinessRule
{
    public abstract string Message { get; }

    public abstract bool IsBroken();

    public override string ToString() => $"{GetType().Name}: {Message}";
}
