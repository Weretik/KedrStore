namespace Application.Common.Results;

public class AppResult : IApplicationResult
{
    public bool IsSuccess { get; }
    public AppError? Error { get; }

    protected AppResult(bool isSuccess, AppError? error)
    {
        if (isSuccess && error != null)
        {
            throw new InvalidOperationException("Success result cannot have an error.");
        }

        if (!isSuccess && error == null)
        {
            throw new InvalidOperationException("Failure result must have an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public static AppResult Success() =>
        new(true, null);

    public static AppResult Failure(AppError error) =>
        new(false, error ?? throw new ArgumentNullException(nameof(error)));

    public static AppResult<T> Success<T>(T value) =>
        AppResult<T>.Success(value);

    public static AppResult<T> Failure<T>(AppError error) =>
        AppResult<T>.Failure(error);

    public static object CreateFailureResult(Type valueType, AppError error)
    {
        if (valueType == null) throw new ArgumentNullException(nameof(valueType));
        if (error == null) throw new ArgumentNullException(nameof(error));

        var resultType = typeof(AppResult<>).MakeGenericType(valueType);
        return Activator.CreateInstance(resultType, false, null, error)!;
    }
}
