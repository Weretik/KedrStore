namespace Domain.Common.Abstractions;

public interface IBusinessRule
{
    string Message { get; }
    bool IsBroken();
}
