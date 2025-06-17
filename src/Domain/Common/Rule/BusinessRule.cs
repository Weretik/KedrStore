namespace Domain.Common.Rule;

public abstract class BusinessRule : IBusinessRule
{

    public abstract string Message { get; }

    public abstract bool IsBroken();

    public bool IsValid() => !IsBroken();
}
