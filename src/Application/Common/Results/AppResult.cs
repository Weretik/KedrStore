namespace Application.Common.Results;

public class AppResult
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
}

public class AppResult<T> : AppResult
{
    private readonly T? _value;
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of failed result.");

    private AppResult(bool isSuccess, T? value, AppError? error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public static AppResult<T> Success(T value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "Success value cannot be null.");
        }
        return new(true, value, null);
    }

    public static AppResult<T> Failure(AppError error) =>
        new(false, default, error ?? throw new ArgumentNullException(nameof(error)));

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<AppError, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess ? onSuccess(Value) : onFailure(Error!);
    }

    public void Match(
        Action<T> onSuccess,
        Action<AppError> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (IsSuccess) onSuccess(Value);
        else onFailure(Error!);
    }
}
