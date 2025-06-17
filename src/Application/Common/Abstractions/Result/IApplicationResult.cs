namespace Application.Common.Abstractions.Result;

public interface IApplicationResult
{
    bool IsSuccess { get; }
    AppError? Error { get; }
}
