namespace Application.Common.Results;

public class ApplicationResult
{
    public bool IsSuccess { get; }
    public AppError? Error { get; }

    protected ApplicationResult(bool isSuccess, AppError? error)
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

    public static ApplicationResult Success() =>
        new(true, null);

    public static ApplicationResult Failure(AppError error) =>
        new(false, error ?? throw new ArgumentNullException(nameof(error)));

    public static ApplicationResult<T> Success<T>(T value) =>
        ApplicationResult<T>.Success(value);

    public static ApplicationResult<T> Failure<T>(AppError error) =>
        ApplicationResult<T>.Failure(error);
}

public class ApplicationResult<T> : ApplicationResult
{
    private readonly T? _value;
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of failed result.");

    private ApplicationResult(bool isSuccess, T? value, AppError? error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public static ApplicationResult<T> Success(T value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "Success value cannot be null.");
        }
        return new(true, value, null);
    }

    public static ApplicationResult<T> Failure(AppError error) =>
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
