namespace Domain.Common.Abstractions;

public interface IBusinessRule
{
    bool IsBroken();

    string Message { get; }
}
